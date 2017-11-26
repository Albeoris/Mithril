using System;
using System.IO;
using Mithril;

namespace Mithril
{
    public static class BinaryReaderExm
    {
        public static Int32 ReadBigInt32(this BinaryReader self)
        {
            Byte[] buff = self.ReadBytes(4);
            unsafe
            {
                fixed (Byte* b = &buff[0])
                    return Endian.ToBigInt32(b);
            }
        }

        public static UInt32 ReadBigUInt32(this BinaryReader self)
        {
            Byte[] buff = self.ReadBytes(4);
            unsafe
            {
                fixed (Byte* b = &buff[0])
                    return Endian.ToBigUInt32(b);
            }
        }

        public static void DangerousReadStructs<T>(this Stream input, T[] output, Int32 count) where T : struct
        {
            if (count < 1)
                return;

            if (count < output.Length)
                throw new ArgumentOutOfRangeException(nameof(count));

            Int32 entrySize = UnsafeTypeCache<T>.UnsafeSize;
            Int32 sizeToRead = entrySize * count;
            using (UnsafeTypeCache<Byte>.ChangeArrayTypes(output, entrySize))
                input.EnsureRead((Byte[])(Array)output, 0, sizeToRead);
        }

        public static void DangerousWriteStructs<T>(this Stream output, T[] input, Int32 count) where T : struct
        {
            if (count < 1)
                return;

            if (count > input.Length)
                throw new ArgumentOutOfRangeException(nameof(count));

            Int32 entrySize = UnsafeTypeCache<T>.UnsafeSize;
            Int32 sizeToWrite = entrySize * count;
            using (UnsafeTypeCache<Byte>.ChangeArrayTypes(input, entrySize))
                output.Write((Byte[])(Array)input, 0, sizeToWrite);
        }

        public static void EnsureRead(this Stream self, Byte[] buff, Int32 offset, Int32 size)
        {
            Int32 readed;
            while (size > 0 && (readed = self.Read(buff, offset, size)) != 0)
            {
                size -= readed;
                offset += readed;
            }

            if (size != 0)
                throw new Exception("Неожиданный конец потока.");
        }
    }
}