using CardWar.Network.Abstractions;
using System.Threading.Tasks;

namespace CardWar.Network.Server
{
    public abstract class PacketHandler<T> : IPacketHandler<T> where T : Packet
    {
        private readonly IPacketConverter _packetConverter;

        public PacketHandler(IPacketConverter packetConverter)
        {
            _packetConverter = packetConverter;
        }

        public async Task Handle(IConnection connection, Packet packet)
        {
            var unwrapped = _packetConverter.Unwrap<T>(packet);

            await this.HandleImp(connection, unwrapped);
        }

        public abstract Task HandleImp(IConnection connection, T Packet);
    }
}
