using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Recommendations.API.Model.ViewModels;
using Recommendations.API.Services;
using Recommendations.DB;

namespace Recommendations.API.Controller
{
    [Authorize]
    [ApiController]
    public class UserController : ControllerBase
    {
        readonly DBClient _client;
        readonly IRecommendationService _recommendationService;

        public UserController(DBClient client, IRecommendationService recommendationService)
        {
            _client = client;
            _recommendationService = recommendationService;
        }

        [HttpGet("user/{id}")]
        [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUser(int id)
        {
            var model = await _client.GetUser(id);

            if (model == null)
                return NotFound();

            return Ok(new User
            {
                ID = model.ID,
                Age = model.Age,
                Sex = model.Sex,

                OrdersLink = Url.Action(
                    "GetUserOrders",
                    "User",
                    new { id },
                    Request.Scheme
                ),

                RecommendationLink = Url.Action(
                    "GetUserRecommendations",
                    "User",
                    new { id },
                    Request.Scheme
                )
            });
        }

        [HttpGet("user/{id}/orders")]
        [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IEnumerable<string>> GetUserOrders(int id)
        {
            const int dummyLimit = 1024;

            var orderIDs = await _client.GetUserOrders(id, 0, dummyLimit);

            return orderIDs.ConvertAll(orderID => Url.Action(
                "GetOrder",
                "Order",
                new { id = orderID },
                Request.Scheme
            ));
        }

        [HttpGet("user/{id}/recommendations")]
        [ProducesResponseType(typeof(Recommendation[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IEnumerable<Recommendation>> GetUserRecommendations(int id)
        {
            var recommendations = await _recommendationService.Get(id);

            return recommendations.Select(x => new Recommendation
            {
                Score = x.Score,
                ProductLink = Url.Action(
                    "GetProduct",
                    "Product",
                    new { id = x.ProductID },
                    Request.Scheme
                )
            }).ToList();
        }
    }
}
