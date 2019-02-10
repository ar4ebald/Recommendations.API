using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Recommendations.DB;

namespace Recommendations.API.Controller
{
    [ApiController]
    [Authorize]
    public class SettingsController : ControllerBase
    {
        readonly DBClient _client;

        public SettingsController(DBClient client)
        {
            _client = client;
        }

        [HttpGet("settings")]
        public async Task<IActionResult> GetSettings()
        {
            if (!int.TryParse(User.Identity.Name, out var operatorID))
                return BadRequest("Invalid token. User name must be an integer");

            var settings = await _client.GetSettings(operatorID);
            if (settings == null)
                return NotFound();

            return Content(settings, "application/json");
        }

        [HttpPut("settings")]
        public async Task<IActionResult> SetSettings([FromBody] JToken json)
        {
            if (!int.TryParse(User.Identity.Name, out var operatorID))
                return BadRequest("Invalid token. User name must be an integer");

            var settingsJson = json.ToString();
            await _client.SetSettings(operatorID, settingsJson);

            return NoContent();
        }
    }
}
