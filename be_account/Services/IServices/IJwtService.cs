using be_account.Models;
using System.Security.Claims;

namespace be_account.Services.IServices
{
    public interface IJwtService
    {
        string CreateJWTToken(User user, List<string> roles);
        string CreateJWTRefresh(User user, List<string> roles);
        public string GenerateJwtTokenGG(ClaimsPrincipal principal, List<string> roles);
        public bool ExpriedToken(string token);
        string ExtractEmailFromToken(string token);
    }
}
