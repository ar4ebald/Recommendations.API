using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Recommendations.API.Model;
using Recommendations.API.Model.Settings;
using Recommendations.DB;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Recommendations.API.Services
{
    public interface IAuthenticationService
    {
        Task<(AuthenticationStatus Status, string Token)> Authenticate(string login, string password);
    }

    public class AuthenticationService : IAuthenticationService
    {
        readonly DBClient _client;
        readonly SigningCredentials _credentials;
        readonly JwtSecurityTokenHandler _tokenHandler;

        public AuthenticationService(IOptions<GlobalOptions> options, DBClient client)
        {
            _client = client;

            var key = Convert.FromBase64String(options.Value.JWTSecret);
            _credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);

            _tokenHandler = new JwtSecurityTokenHandler();
        }

        public async Task<(AuthenticationStatus Status, string Token)> Authenticate(string login, string password)
        {
            var (@operator, expectedPassword) = await _client.GetOperator(login);

            if (password != expectedPassword)
                return (AuthenticationStatus.InvalidLoginOrPassword, null);

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, @operator.ID.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = _credentials
            };

            var token = _tokenHandler.CreateToken(descriptor);
            var tokenString = _tokenHandler.WriteToken(token);

            return (AuthenticationStatus.OK, tokenString);
        }
    }
}
