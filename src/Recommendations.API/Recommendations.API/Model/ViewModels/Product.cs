namespace Recommendations.API.Model.ViewModels
{
    public class Product
    {
        public int ID { get; set; }
        public string Name { get; set; }
        
        public Category Category { get; set; }
    }
}
