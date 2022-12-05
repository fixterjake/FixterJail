using FixterJail.Shared.Models;

namespace FixterJail.Shared.Scripts
{
    internal class Configuration
    {
        const string CONFIG_LOCATION = $"/config.json";
        private static Config _config = null;

        private static Config GetConfig()
        {
            try
            {
                if (_config is not null)
                    return _config;

                string configFile = LoadResourceFile(GetCurrentResourceName(), CONFIG_LOCATION);
                _config = JsonConvert.DeserializeObject<Config>(configFile);
                return _config;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Configuration was unable to be loaded.");
                return (Config)default!;
            }
        }

        public static Config Get => GetConfig();
    }
}
