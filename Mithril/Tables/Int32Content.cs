using System;
using System.Globalization;
using System.IO;

namespace Mithril
{
    internal class Int32Content : ICellContent
    {
        private readonly UInt32 _value;

        public Int32Content(UInt32 value)
        {
            _value = value;
        }

        public void Write(StreamWriter sw)
        {
            sw.Write(_value.ToString(CultureInfo.InvariantCulture));
        }
    }
}