using be_account.DTOs.Requests;
using be_account.Models;
using Microsoft.AspNetCore.Identity;

namespace be_account.Services.IServices
{
    public interface IAccountService
    {
        public Task<User?> FindUserbyEmail(string email);
        public Task<User?> FindUserbyUserName(string username);
        public Task<IdentityResult> CreateUser(UserRegisterDTO userRegister);
        public Task<IEnumerable<User>> GetAll();
  
    }
}
