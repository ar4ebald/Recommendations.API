using Recommendations.API.Model.ViewModels;

namespace Recommendations.API.Model.Settings
{
    public class GlobalConfiguration
    {
        public double? Score { get; set; }
        public int? Count { get; set; }
        public Category[] FilteredCategories { get; set; }
    }
}
