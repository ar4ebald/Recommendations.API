using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recommendations.API.Model.ViewModels;
using Recommendations.DB;

namespace Recommendations.API.Controller
{
    //[Authorize]
    [ApiController]
    public class UserController : ControllerBase
    {
        readonly DBClient _client;

        public UserController(DBClient client)
        {
            _client = client;
        }

        [HttpGet("user/{id}")]
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
                )
            });
        }

        [HttpGet("user/{id}/orders")]
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
    }
}
