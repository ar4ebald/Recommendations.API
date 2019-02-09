using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Recommendations.API.Model;
using Recommendations.API.Model.Requests;
using Recommendations.API.Services;

namespace Recommendations.API.Controller
{
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        readonly IAuthenticationService _authenticationService;

        public AuthenticationController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [HttpPost("authenticate")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Authenticate(AuthenticationRequest request)
        {
            var (status, token) = await _authenticationService.Authenticate(request.Login, request.Password);

            if (status == AuthenticationStatus.OK)
                return Ok(token);

            return Unauthorized(status.ToString());
        }
    }
}
