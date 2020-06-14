using CardWar.Network.Abstractions;
using CardWar.Network.Server;
using CardWar.Packets;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace CardWar.Server.PacketHandlers
{
    public class PingRequestPacketHandler : PacketHandler<PingRequestPacket>
    {
        private readonly ILogger _logger;

        public PingRequestPacketHandler(ILogger<PingRequestPacketHandler> logger, IPacketConverter packetSerializer) : base(packetSerializer)
        {
            _logger = logger;
        }

        public override async Task HandleImp(IConnection connection, PingRequestPacket packet)
        {
            _logger.LogInformation($"{packet} from {connection.Id}");

            await connection.Send(new PingResponsePacket { });
        }
    }
}
