﻿using System;
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

        public void Write(FileStream decOutput, BinaryWriter decBw)
        {
            decBw.WriteBig(_value);
        }

        public void Write(CsvWriter cw)
        {
            cw.String(String.Format("0x{0:X8}", _value));
        }
    }
}