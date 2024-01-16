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
        private const string FILENAME = "appsettings.json";

        static AppSettings()
        {
            _config = ForceReload();
        }

        public static IConfigurationRoot ForceReload()
        {
            _config = new ConfigurationBuilder()
                .AddJsonFile(FILENAME, optional: false, reloadOnChange: true)
                .Build();

            return _config;
        }

        public static string Get(string path)
        {
            if (path.StartsWith("settings:"))
            {
                return _config[path] ?? string.Empty;
            }

            return _config.GetSection("settings")[path] ?? string.Empty;
        }

        public static IConfigurationRoot GetAll()
        {
            return _config;
        }
    }
}
