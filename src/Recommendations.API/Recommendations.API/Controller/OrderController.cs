using System.Linq;
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
    public class OrderController : ControllerBase
    {
        readonly DBClient _client;

        public OrderController(DBClient client)
        {
            _client = client;
        }

        [HttpGet("order/{id}")]
        [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrder(int id)
        {
            var (model, productIDs) = await _client.GetOrder(id);

            if (model == null)
                return NotFound();

            var products = await _client.GetProducts(productIDs);

            var categoryIDs = products.Select(x => x.CategoryID).Distinct().ToList();
            var categories = await _client.GetCategories(categoryIDs);

            var categoryByID = categories.ToDictionary(
                x => x.ID,
                x => new Category { ID = x.ID, Name = x.Name }
            );

            var order = new Order
            {
                ID = model.ID,
                Day = model.Day,

                Products = products.ConvertAll(product => new Product
                {
                    ID = product.ID,
                    Name = product.Name,
                    Category = categoryByID[product.CategoryID]
                })
            };

            return Ok(order);
        }
    }
}
