using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recommendations.API.Model;
using Recommendations.API.Services;

namespace Recommendations.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        readonly IAuthenticationService _authenticationService;

        public TestController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [HttpGet]
        [Authorize]
        public string Foo() => "Bar";

        [HttpPost]
        public async Task<IActionResult> Authenticate(string login, string password)
        {
            var (status, token) = await _authenticationService.Authenticate(login, password);

            if (status == AuthenticationStatus.OK)
                return Ok(token);

            return Unauthorized(status.ToString());
        }
    }
}
