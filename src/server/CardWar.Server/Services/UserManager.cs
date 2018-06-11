using Microsoft.Extensions.Logging;
using CardWar.Server.Data;
using System.Threading.Tasks;

namespace CardWar.Server.Services
{
    class UserManager
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger _logger;

        public UserManager(ApplicationDbContext dbContext, ILoggerFactory loggerFactory)
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