using System;
using System.Threading;

namespace Server
{
    class Program
    {
        static void Main()
        {
            Thread t = new Thread(delegate ()
            {
                new Server("0.0.0.0", 12345).Run();
            });
            t.Start();

            Console.WriteLine("Server Started...!");
        }
    }
}
