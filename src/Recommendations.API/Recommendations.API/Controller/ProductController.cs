using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recommendations.API.Model.ViewModels;
using Recommendations.DB;

namespace Recommendations.API.Controller
{
    //[Authorize]
    [ApiController]
    [ResponseCache(Duration = CacheConstants.Duration)]
    public class ProductController : ControllerBase
    {
        readonly DBClient _client;

        public ProductController(DBClient client)
        {
            _client = client;
        }

        [HttpGet("product/{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var model = await _client.GetProduct(id);

            if (model == null)
                return NotFound();

            return Ok(new Product
            {
                ID = model.ID,
                Name = model.Name,
                CategoryLink = Url.Action(
                    "GetCategory",
                    "Category",
                    new { id = model.CategoryID },
                    Request.Scheme
                )
            });
        }
    }
}
