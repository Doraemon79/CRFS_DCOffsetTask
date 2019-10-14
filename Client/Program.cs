using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using Common;

namespace Client
{
    class Program
    {
        private static readonly int BufferSize = 1000000 * Item.SizeOf;

        static void Main()
        {
            //Item[] items = new[] { new Item(0x1234, 2), new Item(-32000, 4) };
            //Byte[] data = items.ToBytes();
            //Item[] destItems = data.ToItems();

            const int clientCount = 1;
            for (int i = 0; i < clientCount; i++)
            {
                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    Connect("127.0.0.1");
                }).Start();
            }

            Console.ReadLine();
        }

        static void Connect(String server)
        {
            try
            {
                const Int32 port = 12345;
                TcpClient client = new TcpClient(server, port);
                NetworkStream stream = client.GetStream();

                const double frequency = 0.01;
                const double phase = 3;
                Sequence sequence = new Sequence(
                    new Signal(10, frequency, phase, 1, 100),
                    new Signal(20, frequency, phase + Math.PI / 2, 2, 200)
                );

                {
                    var initialBytes = sequence.GetItems(Item.Latency).ToBytes();
                    stream.Write(initialBytes, 0, initialBytes.Length);
                }

                const int expectedCount = 1000000;
                var sendItems = sequence.GetItems(expectedCount);
                var bytes = sendItems.ToBytes();

                Stopwatch stopWatch = Stopwatch.StartNew();
                const int transactionCount = 100;
                for (int i = 0; i < transactionCount; i++)
                {
                    stream.Write(bytes, 0, bytes.Length);

                    int offset = 0;
                    byte[] buffer = new byte[BufferSize];
                    int receivedCount = 0;
                    while (client.Connected && receivedCount < expectedCount)
                    {
                        var numberOfBytesRead = stream.Read(buffer, offset, buffer.Length - offset);

                        //If client closed connection Read returns 0.
                        if (numberOfBytesRead == 0)
                            break;

                        int readyCount = (numberOfBytesRead + offset) / Item.SizeOf;
                        receivedCount += readyCount;
                        if (readyCount > 0)
                        {
                            //var items = buffer.ToItems(readyCount);
                            //foreach (var item in items)
                            //{
                            //    Console.WriteLine(item);
                            //}
                        }

                        //Move rest to beginning of buffer.
                        int extraByteCount = numberOfBytesRead + offset - readyCount * Item.SizeOf;
                        int startOffset = numberOfBytesRead + offset - extraByteCount;
                        Buffer.BlockCopy(buffer, startOffset, buffer, 0, extraByteCount);
                        offset = extraByteCount;
                    }
                }
                stopWatch.Stop();
                TimeSpan ts = stopWatch.Elapsed;
                var megabytesPerSecond = transactionCount * expectedCount * Item.SizeOf / ts.TotalSeconds / 1048576;

                Console.WriteLine($"Bandwidth: {megabytesPerSecond:F2} MB/s");
                Console.ReadLine();

                stream.Close();
                client.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e);
            }
        }
    }
}
