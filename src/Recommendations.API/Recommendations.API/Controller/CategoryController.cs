using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recommendations.API.Model.ViewModels;
using Recommendations.DB;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Recommendations.API.Controller
{
    [Authorize]
    [ApiController]
    [ResponseCache(Duration = CacheConstants.Duration)]
    public class CategoryController : ControllerBase
    {
        readonly DBClient _client;

        public CategoryController(DBClient client)
        {
            _client = client;
        }

        [HttpGet("category/{id}")]
        [ProducesResponseType(typeof(Category), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCategory(int id)
        {
            var model = await _client.GetCategory(id);

            if (model == null)
                return NotFound();

            var category = new Category
            {
                ID = model.ID,
                Name = model.Name
            };

            if (model.ParentID != null)
            {
                category.ParentLink = Url.Action(
                    "GetCategory",
                    "Category",
                    new { id = model.ParentID.Value },
                    Request.Scheme
                );
            }

            return Ok(category);
        }
    }
}
