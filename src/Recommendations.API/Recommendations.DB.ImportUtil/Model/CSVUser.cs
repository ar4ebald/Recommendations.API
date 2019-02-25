using CsvHelper.Configuration.Attributes;

namespace Recommendations.DB.ImportUtil.Model
{
    struct CSVUser
    {
        [Name("user_id")]
        public int ID { get; set; }
        [Name("age")]
        public int Age { get; set; }
        [Name("sex")]
        public string Sex { get; set; }
    }
}
