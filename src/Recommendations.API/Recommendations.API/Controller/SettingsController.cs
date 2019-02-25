using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Recommendations.API.Model.Settings;
using Recommendations.API.Services;

namespace Recommendations.API.Controller
{
    [ApiController]
    [Authorize]
    public class SettingsController : ControllerBase
    {
        readonly IConfigurationRepository _configurationRepository;

        public SettingsController(IConfigurationRepository configurationRepository)
        {
            _configurationRepository = configurationRepository;
        }

        [HttpGet("settings")]
        public GlobalConfiguration GetSettings() => _configurationRepository.Instance;

        [HttpPut("settings")]
        public void SetSettings([FromBody] GlobalConfiguration json, [FromServices]IHostingEnvironment env) => _configurationRepository.Instance = json;
    }
}
