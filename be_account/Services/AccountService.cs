// Import project
using be_account.DTOs.Requests;
using be_account.Models;
using be_account.Repositories.IRepositories;


// Import project
using be_account.Services.IServices;
using Microsoft.AspNetCore.Identity;

namespace be_account.Services
{
    public class AccountService : IAccountService
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<User> _userManager;
        public AccountService(IUserRepository userRepository , UserManager<User> userManager)
        {
            _userRepository = userRepository;
            _userManager = userManager;
        }
        public async Task<User?> FindUserbyEmail(string email)
        {
            return await _userRepository.GetAsync(user => user.Email == email);
        }
        public async Task<User?> FindUserbyUserName(string username)
        {
            return await _userRepository.GetAsync(user => user.UserName == username);
        }

        public async Task<IdentityResult> CreateUser(UserRegisterDTO userRegister)
        {
            var user = new User
            {
                Email = userRegister.Email,
                FirstName = userRegister.FirstName,
                LastName = userRegister.LastName,
                FullName = userRegister.LastName + " " + userRegister.FirstName,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = userRegister.UserName
            };

            var IdentityResult = await _userManager.CreateAsync(user, userRegister.Password);
            IdentityResult = await _userManager.AddToRolesAsync(user, userRegister.Roles);
            return IdentityResult;
        }

        public async Task<IEnumerable<User>> GetAll()
        {
            return await _userRepository.GetAllAsync();
        }

    }
}
