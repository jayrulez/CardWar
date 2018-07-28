using System;
using System.Collections.Generic;
using System.Text;

namespace CardWar.Server.Data
{
    public class Session
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string ConnectionId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime LastActiveAt { get; set; }

        public virtual User User { get; set; }
    }
}
