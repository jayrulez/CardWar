using System;
using System.Threading.Tasks;
using CardWar.Network.Abstractions;

namespace CardWar.Network.Server
{
    public abstract class ServerPacketHandler<T> : IServerPacketHandler where T : Packet
    {
        private readonly IPacketSerializer _packetSerializer;

        public ServerPacketHandler(IPacketSerializer packetSerializer)
        {
            _packetSerializer = packetSerializer;
        }

        public string PacketType => typeof(T).Name;

        public async Task Handle(IConnection connection, byte[] packetBytes)
        {
            var packet = _packetSerializer.Deserialize<T>(packetBytes);

            await this.Handle(connection, packet);
        }

        public abstract Task Handle(IConnection connection, T Packet);
    }
}
