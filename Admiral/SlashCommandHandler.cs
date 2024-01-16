using Discord.WebSocket;
using TheKrystalShip.Admiral.Domain;
using TheKrystalShip.Admiral.Services;
using TheKrystalShip.Admiral.Tools;
using TheKrystalShip.Logging;

namespace TheKrystalShip.Admiral
{
    public class SlashCommandHandler
    {
        private readonly Logger<SlashCommandHandler> _logger;
        private readonly CommandExecutioner _commandExecutioner;

        private readonly string[] waitVariants = [
            "give me a sec", "will take a sec", "hold on", "I'll get back to you",
            "hang on", "just a moment", "gimme a minute", "processing", "beep boop", "brb"
        ];

        public event Func<RunningStatusUpdatedArgs, Task>? RunningStatusUpdated;

        public SlashCommandHandler()
        {
            _logger = new();
            _commandExecutioner = new();
        }

        public async Task OnSlashCommandExecuted(SocketSlashCommand command)
        {
            // All commands require the user to specify a game.
            string game = (string)(command.Data.Options?.First()?.Value ?? "");

            // Received command
            _logger.LogInformation("Discord", $">>> /{(command.Data.Name ?? "") + " " + game}");

            Result result = command.Data.Name switch
            {
                Command.START                   => await OnGameStartCommandAsync(command, game),
                Command.STOP                    => await OnGameStopCommandAsync(command, game),
                Command.RESTART                 => await OnGameRestartCommandAsync(command, game),
                Command.CHECK_FOR_UPDATE        => await OnGameCheckForUpdateCommand(command, game),
                Command.STATUS                  => OnGameStatusCommand(game),
                Command.SET_AUTOSTART           => OnGameAutostartCommand(command, game),
                Command.IS_ONLINE               => OnGameIsActiveCommand(game),
                Command.IS_AUTOSTART_ENABLED    => OnGameIsEnabledCommand(game),

                // Default
                _ => new Result(CommandStatus.Error, "Command or Game not found")
            };

            if (result.Status == CommandStatus.Error)
            {
                await command.RespondAsync($"Error: {result.Output}");
                return;
            }

            // Returned result
            _logger.LogInformation("Discord", $"<<< {result.Output}");

            // Force to not respond, assume it was responded somewhere else
            if (result.Status != CommandStatus.Ignore)
                await command.RespondAsync(result.Output.FirstCharToUpper());
        }

        private async Task<Result> OnGameCheckForUpdateCommand(SocketSlashCommand command, string game)
        {
            // Sprinkle some personality
            Random random = new();
            string randomWait = waitVariants[random.Next(waitVariants.Length)];
            string elipses = random.Next() % 2 == 0 ? "..." : "";

            // Checking for update take a while, respond first then follow up once execution is finished
            await command.RespondAsync($"Checking for {game} updates, {randomWait}{elipses}");

            Result result = _commandExecutioner.CheckForUpdate(game);

            // Default answer with no update before checking
            string followupText = $"No update found for {game}";

            // CheckForUpdate returns CommandStatus.Error when there's an update
            if (result.Status == CommandStatus.Error)
            {
                // RunningStatusUpdated?.Invoke(new(game, RunningStatus.NeedsUpdate));
                followupText = $"New version found: {result.Output}";
            }

            await command.FollowupAsync(followupText);

            return new Result(CommandStatus.Ignore, followupText);
        }

        private Result OnGameAutostartCommand(SocketSlashCommand command, string game)
        {
            int newState = Convert.ToInt32(command.Data.Options.ElementAt(1)?.Value);

            Result result = newState switch
            {
                1 => _commandExecutioner.Enable(game),
                0 => _commandExecutioner.Disable(game),

                // Default
                _ => new Result(CommandStatus.Error, "Unknown error")
            };

            if (result.Status == CommandStatus.Success)
            {
                string autostartValue = newState == 1 ? "enabled" : "disabled";
                return new Result($"Autostart {autostartValue} for {game}");
            }

            return new Result(CommandStatus.Error, $"Failed to set autostart for {game}");
        }

        private Result OnGameIsActiveCommand(string game)
        {
            return _commandExecutioner.IsActive(game);
        }

        private Result OnGameIsEnabledCommand(string game)
        {
            Result result = _commandExecutioner.IsEnabled(game);
            return new Result(CommandStatus.Success, $"Autostart is {result.Output.Trim()} for {game}");
        }

        private async Task<Result> OnGameStartCommandAsync(SocketSlashCommand command, string game)
        {
            await command.RespondAsync($"Starting {game}");

            Result result = _commandExecutioner.Start(game);

            if (result.Status == CommandStatus.Success)
            {
                RunningStatusUpdated?.Invoke(new(game, RunningStatus.Online));
                result.Output = $"Started {game}";
            }

            return new Result(CommandStatus.Ignore, result.Output);
        }

        private async Task<Result> OnGameStopCommandAsync(SocketSlashCommand command, string game)
        {
            await command.RespondAsync($"Stopping {game}...");

            Result result = _commandExecutioner.Stop(game);

            if (result.Status == CommandStatus.Success)
            {
                RunningStatusUpdated?.Invoke(new(game, RunningStatus.Offline));
                result.Output = $"Stopped {game}";
            }

            return new Result(CommandStatus.Ignore, result.Output);
        }

        private async Task<Result> OnGameRestartCommandAsync(SocketSlashCommand command, string game)
        {
            await command.RespondAsync($"Restarting {game}...");

            Result result = _commandExecutioner.Restart(game);

            if (result.Status == CommandStatus.Success)
                result.Output = $"Restarted {game}";

            return new Result(CommandStatus.Ignore, result.Output);
        }

        private Result OnGameStatusCommand(string game)
        {
            return _commandExecutioner.Status(game);
        }
    }
}
