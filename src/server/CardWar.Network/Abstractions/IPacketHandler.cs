using System.Threading.Tasks;

namespace CardWar.Network.Abstractions
{
    public interface IPacketHandler
    {
        Task Handle(IConnection connection, Packet packet);
    }

    public interface IPacketHandler<T> : IPacketHandler where T : Packet
    {
    }
}
