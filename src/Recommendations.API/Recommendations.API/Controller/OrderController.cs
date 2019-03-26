using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Recommendations.API.Model.ViewModels;
using Recommendations.DB;
using Recommendations.DB.ImportUtil.Model;

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
                    Age = product.Age,
                    Sex = product.Sex,
                    Category = categoryByID[product.CategoryID]
                })
            };

            return Ok(order);
        }

        [HttpPost("order/parse")]
        public async Task<List<Order>> ParseOrders()
        {
            List<CSVOrder> csvOrders;

            using (var reader = new StreamReader(Request.Body))
            using (var csvReader = new CsvReader(reader))
                csvOrders = csvReader.GetRecords<CSVOrder>().ToList();

            var productIDs = csvOrders.Select(x => x.ProductID).Distinct().ToList();
            var products = await _client.GetProducts(productIDs);
            var productByID = products.ToDictionary(
                x => x.ID,
                x => new Product { ID = x.ID, Age = x.Age, Sex = x.Sex, Name = x.Name }
            );

            var orders = csvOrders
                .GroupBy(x => x.OrderID)
                .Select(group => new Order
                {
                    ID       = group.Key,
                    Day      = group.First().Day,
                    Products = group.Select(x => productByID[x.ProductID]).ToList()

                })
                .ToList();

            return orders;
        }
    }
}
