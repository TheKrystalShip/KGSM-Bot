using Microsoft.Extensions.Configuration;

namespace TheKrystalShip.Admiral.Tools
{
    /// <summary>
    /// Ensures appsettings.json exists and it doesn't contain empty fields
    /// </summary>
    public class AppSettingsVerifier
    {
        static AppSettingsVerifier()
        {
            CreateDefaultIfNone();
        }

        /// <summary>
        /// Verifies that the appsettings.json file contains all the required fields
        /// </summary>
        /// <returns></returns>
        public static void Verify()
        {
            AppSettingsVerifier selfInstance = new();

            IConfigurationRoot config = AppSettings.GetAll();

            // Get a list of all empty fields
            List<string> errors = selfInstance.CheckSectionForEmpty(config.GetSection("settings"));

            if (errors.Count > 0)
            {
                throw new InvalidDataException("\nSettings file has errors:\n" + string.Join("\n", errors));
            }
        }

        /// <summary>
        /// Recursively check all setting entries for empty values
        /// </summary>
        /// <param name="section">Section name</param>
        /// <returns>A list of errors, could be empty</returns>
        private List<string> CheckSectionForEmpty(IConfigurationSection section)
        {
            List<string> errors = [];

            if (section.Value is null)
            {
                foreach (var entry in section.GetChildren())
                {
                    return CheckSectionForEmpty(entry);
                }
            }

            // Check for empty values
            if (section.Value == string.Empty)
            {
                errors.Add($"Section '{section.Path}' is empty\n");
            }

            return errors;
        }

        /// <summary>
        /// Creates a default "appsettings.json" file if none is found.
        /// </summary>
        /// <exception cref="FileNotFoundException">Thrown if default settings file is not found</exception>
        private static void CreateDefaultIfNone()
        {
            bool settingsFileExists = File.Exists(AppSettings.FILENAME);

            // If there's an existing settings file do nothing
            if (settingsFileExists)
            {
                return;
            }

            bool defaultSettingsFileExists = File.Exists(AppSettings.DEFAULT_FILENAME);

            // If there isn't a default, but there's a settings file also do nothing
            if (!defaultSettingsFileExists && settingsFileExists)
            {
                return;
            }

            // If neither file is present throw exception
            if (!defaultSettingsFileExists && !settingsFileExists)
            {
                throw new FileNotFoundException($"Default {AppSettings.DEFAULT_FILENAME} file doesn't exists");
            }

            // Create a settings file using the default as a template
            Console.WriteLine($"No {AppSettings.FILENAME} file was found, creating new using default");

            File.Copy(AppSettings.DEFAULT_FILENAME, AppSettings.FILENAME, overwrite: false);
        }
    }
}
