using CsvHelper.Configuration.Attributes;

namespace Recommendations.DB.ImportUtil.Model
{
    public struct CSVOrder
    {
        [Name("order_id")]
        public int OrderID { get; set; }
        [Name("product_id")]
        public int ProductID { get; set; }
        [Name("user_id")]
        public int UserID { get; set; }
        [Name("day")]
        public int Day { get; set; }
    }
}
