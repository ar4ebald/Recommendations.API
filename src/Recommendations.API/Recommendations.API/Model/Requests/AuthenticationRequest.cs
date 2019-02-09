using System.ComponentModel.DataAnnotations;

namespace Recommendations.API.Model.Requests
{
    public sealed class AuthenticationRequest
    {
        [Required]
        public string Login { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
