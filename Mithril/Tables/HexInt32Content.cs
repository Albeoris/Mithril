using System;
using System.IO;

namespace Mithril
{
    internal class HexInt32Content : ICellContent
    {
        private readonly Int32 _value;

        public HexInt32Content(Int32 value)
        {
            _value = value;
        }

        public void Write(StreamWriter sw)
        {
            sw.Write("0x{0:X8}", _value);
        }
    }
}