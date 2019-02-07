using Microsoft.AspNetCore.Mvc;
using Recommendations.DB;

namespace Recommendations.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        public TestController(DBClient client)
        {
            ;
        }

        [HttpGet]
        public string Foo() => "Bar";
    }
}
