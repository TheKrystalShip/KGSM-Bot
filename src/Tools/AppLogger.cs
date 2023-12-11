using Discord;
using TheKrystalShip.Logging;

namespace TheKrystalShip.Admiral.Tools
{
    public class AppLogger
    {
        private readonly Logger<AppLogger> _logger;

        public AppLogger()
        {
            _logger = new Logger<AppLogger>();
        }

        public Task OnLog(LogMessage message)
        {
            _logger.LogInformation(message.ToString());
            return Task.CompletedTask;
        }
    }
}
