using System;
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
    public class CategoryController : ControllerBase
    {
        const int MaxCategoriesToSearch = 256;
        const int DefaultCategoriesToSearch = 32;

        readonly DBClient _client;

        public CategoryController(DBClient client)
        {
            _client = client;
        }

        Category ConvertToViewModel(Recommendations.Model.Category model)
        {
            var result = new Category
            {
                ID = model.ID,
                Name = model.Name
            };

            if (model.ParentID != null)
            {
                result.ParentLink = Url.Action(
                    "GetCategory",
                    "Category",
                    new { id = model.ParentID.Value },
                    "https"
                );
            }

            return result;
        }

        [HttpGet("category/{id}")]
        [ProducesResponseType(typeof(Category), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResponseCache(Duration = CacheConstants.Duration)]
        public async Task<IActionResult> GetCategory(int id)
        {
            var model = await _client.GetCategory(id);

            if (model == null)
                return NotFound();

            return Ok(ConvertToViewModel(model));
        }

        [HttpGet("category/search")]
        [ProducesResponseType(typeof(Category[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SearchCategory(string query, ushort? limit)
        {
            var actualLimit = limit == null ? DefaultCategoriesToSearch : Math.Min((int)limit, MaxCategoriesToSearch);
            var models = await _client.SearchCategory(query ?? string.Empty, actualLimit);
            return Ok(models.ConvertAll(ConvertToViewModel));
        }
    }
}
