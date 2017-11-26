using System;
using System.IO;

namespace Mithril
{
    public static class BinaryWriterExm
    {
        public static void WriteBig(this BinaryWriter self, Int32 value)
        {
            self.Write(Endian.SwapInt32(value));
        }

        public static void WriteBig(this BinaryWriter self, UInt32 value)
        {
            self.Write(Endian.SwapUInt32(value));
        }
    }
}