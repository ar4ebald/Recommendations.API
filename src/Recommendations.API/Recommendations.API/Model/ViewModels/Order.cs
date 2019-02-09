namespace Recommendations.API.Model.ViewModels
{
    public class Order
    {
        public int ID { get; set; }
        public int Day { get; set; }

        public string UserLink { get; set; }

        public string[] ProductsLinks { get; set; }
    }
}