using System;
using System.Globalization;
using System.IO;

namespace Mithril
{
    internal class Int32ContentWithoutOffset : ICellContent
    {
        private readonly UInt32 _value;

        public Int32ContentWithoutOffset(UInt32 value)
        {
            _value = value;
        }

        public void Write(FileStream decOutput, BinaryWriter decBw)
        {
        }

        public void Write(CsvWriter cw)
        {
            cw.UInt32(_value);
        }
    }
}