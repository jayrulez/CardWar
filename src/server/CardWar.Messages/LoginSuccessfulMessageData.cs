using System;

namespace CardWar.Messages
{
    public class LoginSuccessfulMessageData
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Token { get; set; }
    }
}
