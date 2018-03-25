using Newtonsoft.Json;
using CardWar.Messages;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using CardWar.Common.Utilities.Extensions;
using CardWar.Common.Messaging;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace CardWar.TestClient
{
    public enum GameState
    {
        Loading,
        Login,
        World
    }

    public class Player
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Token { get; set; }
    }

    class Program
    {
        static TcpClient Client;
        static Player Player { get; set; }
        static GameState GameState = GameState.Loading;

        const int HeartBeatInterval = 2000;

        static async Task Main(string[] args)
        {
            Thread.Sleep(5000);

            try
            {
                while (true)
                {
                    switch (GameState)
                    {
                        case GameState.Loading:
                            if(Client == null || !Client.Connected)
                            {
                                Console.WriteLine("Connecting...");
                                Client = new TcpClient("127.0.0.1", 5555);
                                Console.WriteLine("Connected.");
                            }

                            if (Player == null)
                            {
                                GameState = GameState.Login;
                            }
                            else
                            {
                                GameState = GameState.World;
                            }
                            break;

                        case GameState.Login:
                            Player = await Login();

                            if (Player != null)
                            {
                                GameState = GameState.World;
                            }
                            break;

                        case GameState.World:
                            WorldLoop();
                            break;
                    }
                }
            }
            finally
            {
                Client.Close();
            }

            //Console.ReadKey();
        }

        public static void HeartBeat(CancellationToken cancellationToken)
        {
            for(; ; )
            {
                if(cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var buffer = new byte[4096];

                var message = new Message
                {
                    Type = (int)MessageType.Heartbeat
                };

                try
                {
                    message.Data = new HeartBeatMessageData
                    {
                        Timestamp = DateTime.UtcNow.ToUnixTimestamp()
                    };

                    var messageString = JsonConvert.SerializeObject(message);

                    var messageBytes = Encoding.UTF8.GetBytes(messageString);

                    Stream stream = Client.GetStream();

                    stream.WriteAsync(messageBytes, 0, messageBytes.Length);

                    stream.Flush();

                    stream.Read(buffer, 0, buffer.Length);

                    var bufferString = Encoding.UTF8.GetString(buffer);

                    Console.WriteLine(bufferString.Trim('\0'));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                Thread.Sleep(HeartBeatInterval);
            }
        }

        public static void WorldLoop()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            var task = Task.Factory.StartNew(() => HeartBeat(cancellationToken), cancellationToken);

            var stopwatch = new Stopwatch();

            stopwatch.Start();

            while(true)
            {
                Console.WriteLine("World");

                if(stopwatch.ElapsedMilliseconds > 10*1000)
                {
                    break;
                }
            }

            cancellationTokenSource.Cancel();

            task.Wait();

            task.Dispose();

            GameState = GameState.Loading;
        }

        public static async Task<Player> Login()
        {
            var PlayerId = Guid.NewGuid();

            var message = new Message
            {
                Type = (int)MessageType.Login,
                Data = new LoginMessageData
                {
                    Username = $"Player_{PlayerId}",
                    Password = "password"
                }
            };

            var messageBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

            Stream stream = Client.GetStream();

            await stream.WriteAsync(messageBytes, 0, messageBytes.Length);

            stream.Flush();

            var buffer = new byte[2048];

            await stream.ReadAsync(buffer, 0, buffer.Length);

            var response = Encoding.UTF8.GetString(buffer);

            var responseToken = JToken.Parse(response);

            var type = (int)responseToken["Type"];
            var data = responseToken["Data"];

            if (type == (int)MessageType.LoginSuccessful)
            {
                var loginResultMessage = JsonConvert.DeserializeObject<LoginSuccessfulMessageData>(data.ToString());


                var player = new Player()
                {
                    Id = loginResultMessage.Id,
                    Username = loginResultMessage.Username,
                    Token = loginResultMessage.Token
                };

                return player;
            }
            else
            {
                return null;
            }
        }
    }
}
