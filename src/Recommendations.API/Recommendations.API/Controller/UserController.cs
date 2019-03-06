using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
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
        readonly IConfigurationRepository _configurationRepository;

        public UserController(DBClient client, IRecommendationService recommendationService, IConfigurationRepository configurationRepository)
        {
            _client = client;
            _recommendationService = recommendationService;
            _configurationRepository = configurationRepository;
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
                    "https"
                ),

                RecommendationLink = Url.Action(
                    "GetUserRecommendations",
                    "User",
                    new { id },
                    "https"
                )
            });
        }

        [HttpGet("user/{id}/orders")]
        [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IEnumerable<Order>> GetUserOrders(int id)
        {
            const int dummyLimit = 1024;

            var orders = await _client.GetUserOrders(id, 0, dummyLimit);
            var productIDs = orders
                .SelectMany(order => order.ProductIDs)
                .Distinct()
                .ToList();

            var products = await _client.GetProducts(productIDs);
            var categoriesIDs = products
                .Select(product => product.CategoryID)
                .Distinct()
                .ToList();

            var categories = await _client.GetCategories(categoriesIDs);

            var categoryByID = categories.ToDictionary(
                x => x.ID,
                x => new Category { ID = x.ID, Name = x.Name }
            );
            var productByID = products.ToDictionary(
                x => x.ID,
                x => new Product
                {
                    ID = x.ID,
                    Name = x.Name,
                    Category = categoryByID[x.CategoryID],
                    Age = x.Age,
                    Sex = x.Sex
                }
            );

            return orders.ConvertAll(order => new Order
            {
                ID = order.Order.ID,
                Day = order.Order.Day,
                Products = Array.ConvertAll(order.ProductIDs, productID => productByID[productID])
            });
        }

        [HttpGet("user/{id}/recommendations")]
        [ProducesResponseType(typeof(Recommendation[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IEnumerable<Recommendation>> GetUserRecommendations(int id)
        {
            var recommendations = await _recommendationService.Get(id);

            var productIDs = recommendations
                .Select(x => x.ProductID)
                .Distinct()
                .ToList();
            var products = await _client.GetProducts(productIDs);

            var additionalProductIDs = products
                .SelectMany(p => p.PurchasedWith)
                .Except(productIDs)
                .Distinct()
                .ToList();

            var additionalProducts = await _client.GetProducts(additionalProductIDs);

            var categoriesIDs = products
                .Concat(additionalProducts)
                .Select(product => product.CategoryID)
                .Distinct()
                .ToList();
            var categories = await _client.GetCategories(categoriesIDs);

            var categoryByID = categories.ToDictionary(
                x => x.ID,
                x => new Category { ID = x.ID, Name = x.Name }
            );
            var productByID = products
                .Concat(additionalProducts)
                .ToDictionary(
                    x => x.ID,
                    x => new Product
                    {
                        ID = x.ID,
                        Name = x.Name,
                        Category = categoryByID[x.CategoryID],
                        Age = x.Age,
                        Sex = x.Sex
                    }
                );

            var purchasedWithByID = products.ToDictionary(x => x.ID, x => x.PurchasedWith);

            var config = _configurationRepository.Instance;

            var filteredCategories = new HashSet<int>(
                (config?.FilteredCategories ?? Array.Empty<Category>()).Select(x => x.ID)
            );
            var minScore = config?.Score ?? double.MinValue;
            var count = config?.Count ?? int.MaxValue;

            return recommendations
                .Where(x => x.Score > minScore && !filteredCategories.Contains(productByID[x.ProductID].Category.ID))
                .OrderByDescending(x => x.Score)
                .Take(count)
                .Select(x => new Recommendation
                {
                    Score   = x.Score,
                    Product = productByID[x.ProductID],

                    PurchasedWith = purchasedWithByID[x.ProductID]
                        .Select(i => productByID[i])
                        .ToArray()
                })
                .ToList();
        }

        [HttpGet("user/all/recommendations")]
        public async Task<IActionResult> GetAllRecommendations()
        {
            var allIds = await _client.GetAllUserIDs();
            allIds = allIds.Take(1000).ToArray();

            var server = new AnonymousPipeServerStream(PipeDirection.Out);

            _ = Task.Run(() => FillPipe(server, allIds));

            var client = new AnonymousPipeClientStream(PipeDirection.In, server.ClientSafePipeHandle);
            return File(client, "text/csv", "report.csv");
        }

        async Task FillPipe(Stream pipe, int[] userIDs)
        {
            var config = _configurationRepository.Instance;

            var filteredCategories = new HashSet<int>(
                (config?.FilteredCategories ?? Array.Empty<Category>()).Select(x => x.ID)
            );
            var minScore = config?.Score ?? double.MinValue;
            var count    = config?.Count ?? int.MaxValue;

            using (var writer = new StreamWriter(pipe))
            {
                await writer.WriteLineAsync("userId;productId;productName;score");
                foreach (var userID in userIDs)
                {
                    var recommendations = await _recommendationService.Get(userID);

                    var productIDs = recommendations.Select(x => x.ProductID).ToList();
                    var productByID = (await _client.GetProducts(productIDs)).ToDictionary(x => x.ID);

                    var filtered = recommendations
                        .Where(x => x.Score > minScore && !filteredCategories.Contains(productByID[x.ProductID].CategoryID))
                        .OrderByDescending(x => x.Score)
                        .Take(count);

                    foreach (var rec in filtered)
                    {
                        var name = productByID[rec.ProductID].Name;
                        name = name.Replace("\"", "\"\"");
                        await writer.WriteLineAsync($"{userID};{rec.ProductID};\"{name}\";{rec.Score}");
                    }
                }
            }
        }
    }
}
