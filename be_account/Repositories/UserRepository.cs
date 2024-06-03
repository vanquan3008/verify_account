using be_account.Data;
using be_account.Models;
using be_account.Repositories.IRepositories;
using TienDaoAPI.Repositories;

namespace be_account.Repositories
{
    public class UserRepository :Repository<User> , IUserRepository
    {
        private readonly DataContext _dbContext;
        public UserRepository(DataContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

    }
}
