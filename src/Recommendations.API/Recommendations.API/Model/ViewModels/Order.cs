using System.Collections.Generic;

namespace Recommendations.API.Model.ViewModels
{
    public class Order
    {
        public int ID { get; set; }
        public int Day { get; set; }

        public IReadOnlyList<Product> Products { get; set; }
    }
}