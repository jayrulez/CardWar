using CardWar.Network.Abstractions;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CardWar.Network.Common
{
    public class TcpConnection : IConnection
    {
        const int BufferSize = 4096;

        public string Id { get; }

        public bool Closed => !_socket.Connected;

        private readonly Socket _socket;
        private readonly IPacketSerializer _packetSerializer;

        public TcpConnection(Socket socket, IPacketSerializer packetSerializer)
        {
            Id = Guid.NewGuid().ToString();

            _socket = socket;
            _packetSerializer = packetSerializer;
        }

        public void Close()
        {
            if (_socket.Connected)
            {
                _socket.Shutdown(SocketShutdown.Both);

                //_socket.Close();
            }
        }

        public async Task Send<T>(T packet) where T : Packet
        {
            var packetBuffer = new byte[BufferSize];

            var serializedPacketBuffer = _packetSerializer.Serialize(packet);
            
            if (serializedPacketBuffer.Length > BufferSize)
            {
                throw new Exception($"Serialized packet is larger than buffer size of '{BufferSize}' bytes.");
            }

            serializedPacketBuffer.CopyTo(packetBuffer, 0);

            await _socket.SendAsync(buffer: new ArraySegment<byte>(array: packetBuffer, offset: 0, count: packetBuffer.Length), socketFlags: SocketFlags.None);
            //await _socket.SendAsync(buffer: new ArraySegment<byte>(array: serializedPacketBuffer, offset: 0, count: serializedPacketBuffer.Length), socketFlags: SocketFlags.None);
        }

        public async Task<(Packet Packet, byte[] PacketBytes)> GetPacket()
        {
            var networkStream = new NetworkStream(_socket, false);

            byte[] buffer = new byte[BufferSize];

            await networkStream.ReadAsync(buffer, 0, buffer.Length);

            var packet = _packetSerializer.Deserialize<Packet>(buffer);

            return (packet, buffer);
        }
        
        public T MapPacket<T>(byte[] packetBytes) where T : Packet
        {
            var packet = _packetSerializer.Deserialize<T>(packetBytes);

            return packet;
        }
    }
}
