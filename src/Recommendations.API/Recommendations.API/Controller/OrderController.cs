using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Recommendations.API.Model.ViewModels;
using Recommendations.DB;

namespace Recommendations.API.Controller
{
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
        public async Task<IActionResult> GetOrder(int id)
        {
            var (model, productIDs) = await _client.GetOrder(id);

            if (model == null)
                return NotFound();

            var order = new Order
            {
                ID = model.ID,
                Day = model.Day,

                UserLink = Url.Action(
                    "GetUser",
                    "User",
                    new { id = model.UserID },
                    Request.Scheme
                ),

                ProductsLinks = Array.ConvertAll(productIDs, productID => Url.Action(
                    "GetProduct",
                    "Product",
                    new { id = productID },
                    Request.Scheme
                ))
            };

            return Ok(order);
        }
    }
}
