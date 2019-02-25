using Recommendations.Model;

namespace Recommendations.API.Model.ViewModels
{
    public class Product
    {
        public int ID { get; set; }
        public string Name { get; set; }
        
        public Category Category { get; set; }

        public double Age { get; set; }
        public Sex Sex { get; set; }
    }
}
