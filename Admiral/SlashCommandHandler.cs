using Discord.WebSocket;
using TheKrystalShip.Admiral.Domain;
using TheKrystalShip.Admiral.Services;
using TheKrystalShip.Logging;

namespace TheKrystalShip.Admiral
{
    public class SlashCommandHandler
    {
        private readonly Logger<SlashCommandHandler> _logger;
        private readonly CommandExecutioner _commandExecutioner;

        public event Func<RunningStatusUpdatedArgs, Task>? RunningStatusUpdated;

        public SlashCommandHandler()
        {
            _logger = new();
            _commandExecutioner = new();
        }

        public async Task OnSlashCommandExecuted(SocketSlashCommand command)
        {
            // All commands require the user to specify a game.
            string game = (string) (command.Data.Options?.First()?.Value ?? "");

            Task action = command.Data.Name switch
            {
                "start"     => HandleGameStartCommandAsync(command, game),
                "stop"      => HandleGameStopCommandAsync(command, game),
                "restart"   => HandleGameRestartCommandAsync(command, game),
                "status"    => HandleGameStatusCommandAsync(command, game),
                _           => command.RespondAsync("Command or Game not found")
            };

            _logger.LogInformation($"/{(command.Data.Name ?? "") + " " + game}");

            await action;
        }

        private async Task HandleGameStartCommandAsync(SocketSlashCommand command, string game)
        {
            // Respond quickly then handle the task in the background.
            Task respondTask = command.RespondAsync($"Starting {game}...");

            Result result = _commandExecutioner.Start(game);

            if (result.Status == CommandStatus.Error)
            {
                await command.RespondAsync($"There might have been an error: {result.Output}");
                return;
            }

            await respondTask;
            RunningStatusUpdated?.Invoke(new(game, RunningStatus.Online));
        }

        private async Task HandleGameStopCommandAsync(SocketSlashCommand command, string game)
        {
            // Respond quickly then handle the task in the background.
            Task respondTask = command.RespondAsync($"Stopping {game}...");
            Result result = _commandExecutioner.Stop(game);

            if (result.Status == CommandStatus.Error)
            {
                await command.RespondAsync($"There might have been an error: {result.Output}");
                return;
            }

            await respondTask;
            RunningStatusUpdated?.Invoke(new(game, RunningStatus.Offline));
        }

        private async Task HandleGameRestartCommandAsync(SocketSlashCommand command, string game)
        {
            // Respond quickly then handle the task in the background.
            Task respondTask = command.RespondAsync($"Restarting {game}...");
            Result result = _commandExecutioner.Restart(game);

            if (result.Status == CommandStatus.Error)
            {
                await command.RespondAsync($"There might have been an error: {result.Output}");
                return;
            }

            await respondTask;
        }

        private async Task HandleGameStatusCommandAsync(SocketSlashCommand command, string game)
        {
            Result result = _commandExecutioner.Status(game);

            if (result.Status == CommandStatus.Error)
            {
                await command.RespondAsync($"There might have been an error: {result.Output}");
                return;
            }

            await command.RespondAsync(result.Output == string.Empty ? "Received no output" : result.Output);
            return;
        }
    }
}
