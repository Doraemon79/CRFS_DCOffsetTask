using System;
using System.Runtime.InteropServices;

namespace Common
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Item
    {
        public const int SizeOf = 4;
        public const int Latency = 1000;
        public Item(Int16 i, Int16 q)
        {
            I = i;
            Q = q;
        }

        public Int16 I { get; set; }
        public Int16 Q { get; set; }

        public override string ToString()
        {
            return $"Item({I}, {Q})";
        }
    }
}
