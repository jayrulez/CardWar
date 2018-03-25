using Microsoft.Extensions.Logging;
using CardWar.Server.Data;
using System.Threading.Tasks;

namespace CardWar.Server.Managers
{
    public class UserManager
    {
        private readonly ServerDbContext _dbContext;
        private readonly ILogger _logger;

        public UserManager(ServerDbContext dbContext, ILoggerFactory loggerFactory)
        {
            _dbContext = dbContext;

            _logger = loggerFactory.CreateLogger<UserManager>();
        }

        public Task<User> CreateUser()
        {
            return null;
        }
    }
}
