using CardWar.Network.Abstractions;
using CardWar.Network.Common;
using CardWar.Packets;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace CardWar.TestClient
{
    class Program
    {
        static Socket Socket;

        static async Task Main(string[] args)
        {
            Thread.Sleep(5000);

            Console.WriteLine("Connecting...");
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Socket.Connect("127.0.0.1", 5000);
            Console.WriteLine("Connected.");

            var connection = new TcpConnection(Socket, new JsonPacketSerializer());

            try
            {
                while (true)
                {
                    if (!connection.Closed)
                    {
                        await PingServer(connection);
                    }
                }
            }
            finally
            {
            }

            //Console.ReadKey();
        }

        public static async Task PingServer(IConnection connection)
        {
            while (!connection.Closed)
            {
                try
                {
                    await connection.Send(new PingRequestPacket
                    {
                    });
                    await connection.Send(new PingRequestPacket
                    {
                    });
                    await connection.Send(new PingRequestPacket
                    {
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                Thread.Sleep(5000);
            }
        }
    }
}
