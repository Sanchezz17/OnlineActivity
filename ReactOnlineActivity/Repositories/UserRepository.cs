using System.Linq;
using ReactOnlineActivity.Data;
using ReactOnlineActivity.Models;

namespace ReactOnlineActivity.Repositories
{
    public class UserRepository
    {
        private readonly ApplicationDbContext dbContext;
        
        public UserRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public ApplicationUser FindByName(string userName) => dbContext.Users.First(u => u.UserName == userName);
    }
}