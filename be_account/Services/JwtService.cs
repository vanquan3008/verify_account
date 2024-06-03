//import project
using be_account.Models;
using be_account.Repositories.IRepositories;

//import project
using be_account.Services.IServices;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;


namespace be_account.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        public JwtService(IConfiguration configuration , IUserRepository userRepository)
        {
            _configuration = configuration;
            _userRepository = userRepository;
        }
        public string CreateJWTToken(User user, List<string> roles)
        {
            // Create claim
            if (user.UserName == null)
            {
                throw new ArgumentNullException("Username must be not null");
            }
            var claim = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName)
            };
            foreach (var role in roles)
            {
                claim.Add(new Claim(ClaimTypes.Role, role));
            }
            // create key
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? ""));
            // create credentials 
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            // create token
            var token = new JwtSecurityToken(_configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claim,
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);

        }

        public string CreateJWTRefresh(User user, List<string> roles)
        {
            {
                if (user.UserName == null)
                {
                    throw new ArgumentNullException("Username must be not null");
                }
                var claim = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName)
                };
                foreach (var role in roles)
                {
                    claim.Add(new Claim(ClaimTypes.Role, role));
                }
                // create key
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? ""));
                // create credentials 
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                // create token
                var token = new JwtSecurityToken(_configuration["Jwt:Issuer"],
                    _configuration["Jwt:Audience"],
                    claim,
                    expires: DateTime.Now.AddDays(364),
                    signingCredentials: credentials);


                return new JwtSecurityTokenHandler().WriteToken(token);
            }
        }

        public bool ExpriedToken(string token)
        {

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var expirationDate = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp)?.Value;

            if (expirationDate == null)
            {
                return true;
            }

            var expDateTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expirationDate)).UtcDateTime;
            return expDateTime < DateTime.UtcNow;

        }
        public string GenerateJwtTokenGG(ClaimsPrincipal principal , List<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, principal.FindFirstValue(ClaimTypes.NameIdentifier)),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, principal.Identity.Name)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public string ExtractEmailFromToken(string token)
        {
            throw new NotImplementedException();
        }
    }
}
