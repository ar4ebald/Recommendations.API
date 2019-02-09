using Recommendations.Model;

namespace Recommendations.API.Model.ViewModels
{
    public class User
    {
        public int ID { get; set; }
        public int Age { get; set; }
        public Sex Sex { get; set; }

        public string OrdersLink { get; set; }
        public string RecommendationLink { get; set; }
    }
}
