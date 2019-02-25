using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Recommendations.DB;

namespace Recommendations.API.Controller
{
    [ApiController]
    [Authorize]
    public class SettingsController : ControllerBase
    {
        const string ConfigPath = "globalConfiguration.json";

        readonly DBClient _client;

        public SettingsController(DBClient client)
        {
            _client = client;
        }

        [HttpGet("settings")]
        public IActionResult GetSettings() => File(ConfigPath, "application/json");

        [HttpPut("settings")]
        public async Task<IActionResult> SetSettings([FromBody] JToken json, [FromServices]IHostingEnvironment env)
        {
            var settingsJson = json.ToString();

            await System.IO.File.WriteAllTextAsync(
                Path.Combine(env.WebRootPath, ConfigPath),
                settingsJson
            );

            return NoContent();
        }
    }
}
