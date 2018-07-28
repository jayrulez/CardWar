using System;
using System.Collections.Generic;
using System.Text;

namespace CardWar.Server.Data
{
    public class User
    {
        public Guid Id { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual ICollection<Session> Sessions { get; set; }

        public User()
        {
            Sessions = new HashSet<Session>();
        }
    }
}