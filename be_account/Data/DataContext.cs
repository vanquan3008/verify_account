
//Import source
using be_account.Models;
//Import library
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace be_account.Data
{
    public class DataContext : IdentityDbContext<User, Role ,int>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            var roles = new List<Role>()
            {
                    new Role(){
                            Id = 1,
                            ConcurrencyStamp = "AdminRole",
                            Name = "Admin",
                            NormalizedName = "Admin".ToUpper()
                    },
                    new Role(){
                            Id = 2,
                            ConcurrencyStamp = "ConverterRole",
                            Name = "Converter",
                            NormalizedName = "Converter".ToUpper()
                    },
                    new Role(){
                            Id = 3,
                            ConcurrencyStamp = "ReaderRole",
                            Name = "Reader",
                            NormalizedName = "Reader".ToUpper()
                    }
            };

            builder.Entity<Role>().HasData(roles);
        }

    }
}
