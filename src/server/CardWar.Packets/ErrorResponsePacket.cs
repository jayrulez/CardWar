using CardWar.Network.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace CardWar.Packets
{
    public class ErrorResponsePacket : Packet
    {
        public int ErrorCode { get; set; }
        public string ErrorDescription { get; set; }
    }
}
