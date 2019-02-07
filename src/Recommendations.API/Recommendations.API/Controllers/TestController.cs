using Microsoft.AspNetCore.Mvc;

namespace Recommendations.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public string Foo() => "Bar";
    }
}
