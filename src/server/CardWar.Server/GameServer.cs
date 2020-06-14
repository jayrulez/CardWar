﻿using CardWar.Network.Abstractions;
using CardWar.Network.Server;
using CardWar.Packets;
using CardWar.Server.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CardWar.Server
{
    public class GameServer : AbstractTcpServer
    {
        public GameServer(IHost host) : base(host)
        {
            _logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger<GameServer>();
        }
        

        public bool StartSession(IConnection connection, string packetKey, byte[] packetBuffer)
        {
            if (packetKey == typeof(LoginRequestPacket).Name)
            {
                //var data = connection.MapPacket<LoginRequestPacket>(packetBuffer);
            }

            if (packetKey == typeof(ReconnectRequestPacket).Name)
            {
                //var data = connection.MapPacket<ReconnectRequestPacket>(packetBuffer);
            }
            return false;
        }

        /*
        public async override Task OnConnected(IConnection connection, CancellationToken cancellationToken)
        {
            var packet = await connection.GetPacket();

            var authenticationPackets = new List<string>()
            {
                typeof(LoginRequestPacket).Name,
                typeof(ReconnectRequestPacket).Name
            };

            if (authenticationPackets.Contains(packet.Packet.PacketType))
            {
                if (StartSession(connection, packet.Packet.PacketType, packet.PacketBytes))
                {
                    await base.OnConnected(connection, cancellationToken);
                }
                else
                {
                    await connection.Send(new ErrorResponsePacket
                    {
                        ErrorCode = (int)ErrorCode.AuthenticationFailed,
                        ErrorDescription = $"Authentication failed."
                    });

                    connection.Close();
                }
            }
            else
            {
                await connection.Send(new ErrorResponsePacket
                {
                    ErrorCode = (int)ErrorCode.LoginRequired,
                    ErrorDescription = $"Unexpected packet type received. Please establish a session with the server."
                });

                connection.Close();
            }
        }
        */
    }
}
