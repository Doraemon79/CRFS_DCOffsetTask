using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Common;

namespace Server
{
    class Server
    {
        private static readonly int BufferSize = 1000000 * Item.SizeOf;
        private readonly IPAddress _localAddress;
        private readonly int _port;

        public Server(string ip, int port)
        {
            _localAddress = IPAddress.Parse(ip);
            _port = port;
        }

        public void Handler(Object clientObject)
        {
            if (clientObject is TcpClient client)
            {
                try
                {
                    double sum = 0;
                    int itemCount = 0;
                    var bufferedItems = new Item[Item.Latency];
                    int bufferIndex = 0;

                    var stream = client.GetStream();

                    int offset = 0;
                    byte[] buffer = new byte[BufferSize];

                    // Incoming message may be larger than the buffer size.
                    while (client.Connected)
                    {
                        var numberOfBytesRead = stream.Read(buffer, offset, buffer.Length - offset);

                        //If client closed connection Read returns 0.
                        if (numberOfBytesRead == 0)
                            break;

                        int readyCount = (numberOfBytesRead + offset) / Item.SizeOf;
                        if (readyCount > 0)
                        {
                            var items = buffer.ToItems(readyCount);
                            //foreach (var item in items)
                            //{
                            //    Console.WriteLine(item);
                            //}

                            var reply = new Item[readyCount - Item.Latency + itemCount];
                            int replyIndex = 0;

                            foreach (var item in items)
                            {
                                if (itemCount < Item.Latency)
                                {
                                    itemCount++;
                                }
                                else
                                {
                                    double dc = sum / (2 * Item.Latency);
                                    
                                    var firstItem = bufferedItems[bufferIndex];
                                    reply[replyIndex++] = new Item(
                                        (Int16)Math.Round(firstItem.I - dc),
                                        (Int16)Math.Round(firstItem.Q - dc)
                                    );

                                    sum -= firstItem.I + firstItem.Q;
                                }

                                bufferedItems[bufferIndex] = item;
                                bufferIndex = (bufferIndex + 1) % bufferedItems.Length;

                                sum += item.I + item.Q;
                            }

                            if (reply.Any())
                            {
                                var replyBytes = reply.ToBytes();
                                stream.Write(replyBytes, 0, replyBytes.Length);
                            }
                        }

                        //Move rest to beginning of buffer.
                        int extraByteCount = numberOfBytesRead + offset - readyCount * Item.SizeOf;
                        int startOffset = numberOfBytesRead + offset - extraByteCount;
                        Buffer.BlockCopy(buffer, startOffset, buffer, 0, extraByteCount);
                        offset = extraByteCount;
                    }
                }
                catch (Exception)
                {
                    //Console.WriteLine($"Exception: {e}");
                    client.Close();
                }

                Console.WriteLine("Client stopped");
            }
        }

        public void Run()
        {
            var server = new TcpListener(_localAddress, _port);
            server.Start();

            try
            {
                while (true)
                {
                    Console.WriteLine("Waiting for a connection...");
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("Connected!");

                    new Thread(Handler).Start(client);
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
                server.Stop();
            }
        }
    }
}
