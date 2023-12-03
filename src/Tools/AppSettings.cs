using Microsoft.Extensions.Configuration;

namespace TheKrystalShip.Admiral.Tools
{
    /// <summary>
    /// Reads the configuration from appsettings.json on startup and makes it
    /// accessible with a simple Get method.
    /// Updates the configuration if the file is modified.
    /// </summary>
    public static class AppSettings
    {
        private static IConfigurationRoot _config;
        public const string SETTINGS_FILENAME = "appsettings.json";
        public const string DEFAULT_SETTINGS_FILENAME = "appsettings.example.json";

        static AppSettings()
        {
            _config = ForceReload();
        }

        public static IConfigurationRoot ForceReload()
        {
            _config = new ConfigurationBuilder()
                .AddJsonFile(SETTINGS_FILENAME, optional: false, reloadOnChange: true)
                .Build();

            return _config;
        }

        public static string? Get(string path)
        {
            return _config.GetSection("settings")[path];
        }

        public static IConfigurationRoot GetAll()
        {
            return _config;
        }
    }
}
