namespace Recommendations.API.Model.ViewModels
{
    public class AuthenticationResult
    {
        public string Token { get; set; }
        public string[] Roles { get; set; }
    }
}
