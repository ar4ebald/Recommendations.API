using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recommendations.API.Services
{
    public interface IRecommendationService
    {
        Task<IReadOnlyList<(double Score, int ProductID)>> Get(int userID);
    }

    public class RecommendationService : IRecommendationService
    {
        public async Task<IReadOnlyList<(double Score, int ProductID)>> Get(int userID)
        {
            var random = new Random();
            var count = random.Next(0, 20);

            return Enumerable.Range(0, count)
                .Select(_ => (random.NextDouble(), random.Next(1, 500 + 1)))
                .OrderByDescending(x => x.Item1)
                .ToArray();
        }
    }
}
