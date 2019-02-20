using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Recommendations.API.Model.ViewModels;
using Recommendations.DB;

namespace Recommendations.API.Controller
{
    [Authorize]
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
        [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProduct(int id)
        {
            var model = await _client.GetProduct(id);

            if (model == null)
                return NotFound();

            var category = await _client.GetCategory(id);

            return Ok(new Product
            {
                ID = model.ID,
                Name = model.Name,
                Category = new Category
                {
                    ID = category.ID,
                    Name = category.Name
                }
            });
        }
    }
}
