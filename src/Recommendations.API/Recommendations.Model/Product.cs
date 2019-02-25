namespace Recommendations.Model
{
    public sealed class Product
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int CategoryID { get; set; }
        public double Age { get; set; }
        public Sex Sex { get; set; }
        public int[] PurchasedWith { get; set; }
    }
}
