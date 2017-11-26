using System;
using System.Runtime.CompilerServices;

namespace Mithril
{
    public static class Endian
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int16 SwapInt16(Int16 v)
        {
            return (Int16)(((v & 0xff) << 8) | ((v >> 8) & 0xff));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt16 SwapUInt16(UInt16 v)
        {
            return (UInt16)(((v & 0xff) << 8) | ((v >> 8) & 0xff));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 SwapInt32(Int32 v)
        {
            return (Int32)(((SwapInt16((Int16)v) & 0xffff) << 0x10) | (SwapInt16((Int16)(v >> 0x10)) & 0xffff));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt32 SwapUInt32(UInt32 v)
        {
            return (UInt32)(((SwapUInt16((UInt16)v) & 0xffff) << 0x10) | (SwapUInt16((UInt16)(v >> 0x10)) & 0xffff));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void SwapUInt32(UInt32* v)
        {
            *v = SwapUInt32(*v);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int64 SwapInt64(Int64 v)
        {
            return (Int64)(((SwapInt32((Int32)v) & 0xffffffffL) << 0x20) | (SwapInt32((Int32)(v >> 0x20)) & 0xffffffffL));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt64 SwapUInt64(UInt64 v)
        {
            return (UInt64)(((SwapUInt32((UInt32)v) & 0xffffffffL) << 0x20) | (SwapUInt32((UInt32)(v >> 0x20)) & 0xffffffffL));
        }

        public static unsafe UInt16 ToBigUInt16(Byte* b)
        {
            return (UInt16)(*b << 8 | *(b + 1));
        }

        public static unsafe Int16 ToBigInt16(Byte* b)
        {
            return (Int16)((*b << 8) | (*(b + 1)));
        }

        public static unsafe Int32 ToBigInt32(Byte* b)
        {
            return *b << 24 | *(b + 1) << 16 | *(b + 2) << 8 | *(b + 3);
        }

        public static unsafe UInt32 ToBigUInt32(Byte* b)
        {
            return (UInt32)(*b << 24 | *(b + 1) << 16 | *(b + 2) << 8 | *(b + 3));
        }

        public static unsafe UInt32 ToBigUInt32(Byte[] array, Int32 index)
        {
            fixed (Byte* b = &array[index])
                return ToBigUInt32(b);
        }
    }
}