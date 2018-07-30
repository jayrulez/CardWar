using CardWar.Common.Utilities;
using System;

namespace CardWar.Network.Abstractions
{
    public class Packet : IPacket
    {
        private string _packetId;

        public string PacketId
        {
            get
            {
                if (string.IsNullOrEmpty(_packetId))
                {
                    _packetId = Guid.NewGuid().ToString();
                }

                return _packetId;
            }
        }

        public string Key { get => TypeUtility.GetTypeName(this); }
    }
}
