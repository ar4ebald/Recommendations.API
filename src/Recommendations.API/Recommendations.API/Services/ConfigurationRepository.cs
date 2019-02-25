using System.IO;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using Recommendations.API.Model.Settings;

namespace Recommendations.API.Services
{
    public interface IConfigurationRepository
    {
        string ConfigPath { get; }

        GlobalConfiguration Instance { get; set; }
    }

    public class ConfigurationRepository : IConfigurationRepository
    {
        public string ConfigPath { get; }


        public ConfigurationRepository(IHostingEnvironment env)
        {
            ConfigPath = Path.Combine(env.WebRootPath, "globalConfiguration.json");
        }

        public GlobalConfiguration Instance
        {
            get => JsonConvert.DeserializeObject<GlobalConfiguration>(File.ReadAllText(ConfigPath));
            set => File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(value));
        }
    }
}
