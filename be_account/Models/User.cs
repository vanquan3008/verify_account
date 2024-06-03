using Microsoft.AspNetCore.Identity;

namespace be_account.Models
{
    public class User:IdentityUser<int>
    {

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }
}
