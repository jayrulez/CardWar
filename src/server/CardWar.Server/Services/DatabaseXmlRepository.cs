using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.Extensions.Logging;
using CardWar.Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace CardWar.Server.Services
{
    class DatabaseXmlRepository : IXmlRepository
    {
        private readonly ILogger _logger;
        private readonly ApplicationDbContext _dbContext;

        public DatabaseXmlRepository(ApplicationDbContext dbContext, ILoggerFactory loggerFactory)
        {
            _dbContext = dbContext;
            _logger = loggerFactory.CreateLogger<DatabaseXmlRepository>();
        }

        public IReadOnlyCollection<XElement> GetAllElements()
        {
            var xmlKeys = _dbContext.XmlKeys.ToList();

            var keys = xmlKeys.Select(xmlKey => XElement.Parse(xmlKey.Data)).ToList();

            return keys.AsReadOnly();
        }

        public void StoreElement(XElement element, string friendlyName)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            var xmlKey = new XmlKey()
            {
                Name = friendlyName,
                Data = element.ToString()
            };

            _dbContext.XmlKeys.Add(xmlKey);

            _dbContext.SaveChanges();
        }
    }
}