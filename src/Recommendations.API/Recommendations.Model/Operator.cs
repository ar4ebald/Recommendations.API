namespace Recommendations.Model
{
    public sealed class Operator
    {
        public int ID { get; set; }
        public string Login { get; set; }
        public string[] Roles { get; set; }
    }
}
