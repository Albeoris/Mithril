using System;

namespace Mithril
{
    public struct BigEndianUInt32
    {
        private readonly UInt32 _value;

        private BigEndianUInt32(UInt32 value)
        {
            _value = value;
        }

        public static implicit operator UInt32(BigEndianUInt32 value)
        {
            return Endian.SwapUInt32(value._value);
        }

        public static implicit operator BigEndianUInt32(UInt32 value)
        {
            return new BigEndianUInt32(Endian.SwapUInt32(value));
        }
    }
}