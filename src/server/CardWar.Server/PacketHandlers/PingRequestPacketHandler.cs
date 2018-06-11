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

        public PingRequestPacketHandler(ILoggerFactory loggerFactory, IPacketSerializer packetSerializer) : base(packetSerializer)
        {
            _logger = loggerFactory.CreateLogger<PingRequestPacketHandler>();
        }

        public override async Task Handle(IConnection connection, PingRequestPacket packet)
        {
            _logger.LogInformation(packet.ToString());

            await connection.Send(new PingResponsePacket { });
        }
    }
}
