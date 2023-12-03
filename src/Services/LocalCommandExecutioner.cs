using TheKrystalShip.Admiral.Domain;

namespace TheKrystalShip.Admiral.Services
{
    /// <summary>
    /// Used to run commands locally on the same machine as the game servers
    /// </summary>
    public class LocalCommandExecutioner : ICommandExecutioner
    {
        public LocalCommandExecutioner()
        {

        }

        public CommandExecutionResult Execute(string command)
        {
            throw new NotImplementedException();
        }
    }
}
