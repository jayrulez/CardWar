using Newtonsoft.Json;
using CardWar.Messages;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace CardWar.TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.Sleep(5000);

            Console.WriteLine("Connecting...");

            TcpClient client = new TcpClient("127.0.0.1", 5555);

            Console.WriteLine("Connected.");

            try
            {
                var buffer = new byte[4096];

                var message = new Message
                {
                    Type = MessageType.Ping
                };

                int i = 0;

                for (; ; )
                {
                    try
                    {
                        message.Data = new HelloWorldMessage
                        {
                            Message = $"Hello World: {++i}"
                        };

                        var messageString = JsonConvert.SerializeObject(message);

                        var messageBytes = Encoding.UTF8.GetBytes(messageString);

                        Stream stream = client.GetStream();

                        stream.WriteAsync(messageBytes, 0, messageBytes.Length);

                        stream.Flush();

                        stream.Read(buffer, 0, buffer.Length);

                        var bufferString = Encoding.UTF8.GetString(buffer);

                        Console.WriteLine(bufferString.Trim('\0'));
                    }catch(Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        break;
                    }
                }
            }
            finally
            {
                client.Close();
            }

            //Console.ReadKey();
        }
    }
}
