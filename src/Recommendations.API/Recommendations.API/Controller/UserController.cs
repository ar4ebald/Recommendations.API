using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recommendations.API.Model.ViewModels;
using Recommendations.DB;

namespace Recommendations.API.Controller
{
    //[Authorize]
    [ApiController]
    public class UserController : ControllerBase
    {
        readonly DBClient _client;

        public UserController(DBClient client)
        {
            _client = client;
        }

        [HttpGet("user/{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var model = await _client.GetUser(id);

            if (model == null)
                return NotFound();

            return Ok(new User
            {
                ID = model.ID,
                Age = model.Age,
                Sex = model.Sex
            });
        }
    }
}
