using CsvHelper.Configuration.Attributes;

namespace Recommendations.DB.ImportUtil.Model
{
    class CSVProduct
    {
        [Name("product_id")]
        public int ProductID { get; set; }
        [Name("product_name")]
        public string ProductName { get; set; }

        [Name("aisle_id")]
        public int AisleID { get; set; }
        [Name("aisle")]
        public string Aisle { get; set; }

        [Name("department_id")]
        public int DepartmentID { get; set; }
        [Name("department")]
        public string Department { get; set; }
    }
}
