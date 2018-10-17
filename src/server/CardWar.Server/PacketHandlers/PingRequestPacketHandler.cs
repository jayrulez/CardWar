using CardWar.Network.Abstractions;
using CardWar.Network.Server;
using CardWar.Packets;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace CardWar.Server.PacketHandlers
{
    public class PingRequestPacketHandler : ServerPacketHandler<PingRequestPacket>
    {
        private readonly ILogger _logger;

        public PingRequestPacketHandler(ILogger<PingRequestPacketHandler> logger, IPacketSerializer packetSerializer) : base(packetSerializer)
        {
            _logger = logger;
        }

        public override async Task Handle(IConnection connection, PingRequestPacket packet)
        {
            _logger.LogInformation(packet.ToString());

            await connection.Send(new PingResponsePacket { });
        }
    }
}
