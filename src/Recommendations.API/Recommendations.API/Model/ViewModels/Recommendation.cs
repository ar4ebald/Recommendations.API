namespace Recommendations.API.Model.ViewModels
{
    public class Recommendation
    {
        public double Score { get; set; }
        public Product Product { get; set; }
    }
}
