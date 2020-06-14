﻿using CardWar.Network.Abstractions;
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

            var connection = new TcpConnection(Socket, new JsonPacketConverter());

            await Task.Factory.StartNew(() => GetMessages(connection));

            try
            {
                while (true)
                {
                    if (!connection.IsClosed)
                    {
                        await PingServer(connection);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            finally
            {
            }

            //Console.ReadKey();
        }

        public static async Task GetMessages(IConnection connection)
        {
            while (!connection.IsClosed)
            {
                try
                {
                    await foreach (var packet in connection.GetPackets())
                    {
                        Console.WriteLine($"Packet received: {packet.Type}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public static async Task PingServer(IConnection connection)
        {
            while (!connection.IsClosed)
            {
                try
                {
                    await connection.Send(new PingRequestPacket
                    {
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                Thread.Sleep(100);
            }
        }
    }
}
