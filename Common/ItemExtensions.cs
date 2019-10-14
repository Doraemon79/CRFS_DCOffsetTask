using System;

namespace Common
{
    public static class ItemExtensions
    {
        public static byte[] ToBytes(this Item[] array)
        {
            byte[] bytes = new byte[array.Length * Item.SizeOf];

            int byteIndex = 0;
            foreach (var item in array)
            {
                Int16 i = item.I;
                bytes[byteIndex++] = (byte)(i & 0xFF);
                bytes[byteIndex++] = (byte)(i >> 8);
                Int16 q = item.Q;
                bytes[byteIndex++] = (byte)(q & 0xFF);
                bytes[byteIndex++] = (byte)(q >> 8);
            }

            return bytes;
        }

        public static Item[] ToItems(this byte[] bytes)
        {
            int length = bytes.Length / Item.SizeOf;
            return ToItems(bytes, length);
        }

        public static Item[] ToItems(this byte[] bytes, int length)
        {
            var array = new Item[length];

            int byteIndex = 0;
            for (int index = 0; index < length; index++)
            {
                array[index].I = (short)(bytes[byteIndex++] | bytes[byteIndex++] << 8);
                array[index].Q = (short)(bytes[byteIndex++] | bytes[byteIndex++] << 8);
            }

            return array;
        }
    }
}
