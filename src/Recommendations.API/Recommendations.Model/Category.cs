namespace Recommendations.Model
{
    public sealed class Category
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int? ParentID { get; set; }
    }
}
