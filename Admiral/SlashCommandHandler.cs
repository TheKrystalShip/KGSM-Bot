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
            _logger.LogInformation("Request", $">>> /{(command.Data.Name ?? "") + " " + game}");

            Result result = command.Data.Name switch
            {
                "start"     => await OnGameStartCommandAsync(command, game),
                "stop"      => await OnGameStopCommandAsync(command, game),
                "restart"   => await OnGameRestartCommandAsync(command, game),
                "status"    => OnGameStatusCommand(game),
                "autostart" => OnGameAutostartCommand(command, game),
                "is-active" => OnGameIsActiveCommand(game),
                "is-enabled"=> OnGameIsEnabledCommand(game),

                // Default
                _           => new Result(CommandStatus.Error, "Command or Game not found")
            };

            if (result.Status == CommandStatus.Error)
            {
                await command.RespondAsync($"Error: {result.Output}");
                return;
            }

            // Returned result
            _logger.LogInformation("Response", $"<<< {result.Output}");

            await command.RespondAsync(result.Output.FirstCharToUpper());
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
            return _commandExecutioner.IsEnabled(game);
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

            return result;
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

            return result;
        }

        private async Task<Result> OnGameRestartCommandAsync(SocketSlashCommand command, string game)
        {
            await command.RespondAsync($"Restarting {game}...");

            Result result = _commandExecutioner.Restart(game);

            if (result.Status == CommandStatus.Success)
                result.Output = $"Restarted {game}";

            return result;
        }

        private Result OnGameStatusCommand(string game)
        {
            return _commandExecutioner.Status(game);
        }
    }
}
