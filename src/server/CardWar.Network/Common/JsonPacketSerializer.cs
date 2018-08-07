using CardWar.Common.Utilities;
using CardWar.Network.Abstractions;
using Newtonsoft.Json;
using System;
using System.Text;

namespace CardWar.Network.Common
{
    public class JsonPacketSerializer : IPacketSerializer
    {
        public T Deserialize<T>(byte[] serializedPacketBytes) where T : Packet
        {
            try
            {
                var data = Encoding.UTF8.GetString(serializedPacketBytes).Trim('\0');

                var packet = JsonConvert.DeserializeObject<T>(data);

                return packet;
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        public (T Packet, byte[] PacketBytes) Deserialize<T>(string serializedPacketBytes) where T : Packet
        {
            try
            {
                var packet = JsonConvert.DeserializeObject<T>(serializedPacketBytes);

                return (packet, Encoding.UTF8.GetBytes(serializedPacketBytes));
            }
            catch (Exception)
            {
                return (default(T), default(byte[]));
            }
        }

        public byte[] Serialize(Packet packet)
        {
            packet.PacketType = TypeUtility.GetTypeName(packet);

            var data = JsonConvert.SerializeObject(packet);

            return Encoding.UTF8.GetBytes(data);
        }
    }
}
