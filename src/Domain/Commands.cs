namespace TheKrystalShip.Admiral.Domain
{
    public static class Commands
    {
        public const string START_COMMAND = "systemctl start {0}";
        public const string STOP_COMMAND = "systemctl stop {0}";
        public const string RESTART_COMMAND = "systemctl restart {0}";
        public const string STATUS_COMMAND = "systemctl status {0} | head -n 3";

        public static string Start(string param) => string.Format(START_COMMAND, param);
        public static string Stop(string param) => string.Format(STOP_COMMAND, param);
        public static string Restart(string param) => string.Format(RESTART_COMMAND, param);
        public static string Status(string param) => string.Format(STATUS_COMMAND, param);
    }
}
