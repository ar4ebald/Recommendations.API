using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Recommendations.API.Model.Settings;
using Recommendations.DB;

namespace Recommendations.API.Services
{
    public interface IAuthenticationService
    {
        Task<string> Authenticate(string login, string password);
    }

    public class AuthenticationService
    {
        readonly IOptions<GlobalOptions> _options;
        readonly DBClient _client;

        public AuthenticationService(IOptions<GlobalOptions> options, DBClient client)
        {
            _options = options;
            _client = client;
        }

        public async Task<string> Authenticate(string login, string password)
        {

        }
    }
}
