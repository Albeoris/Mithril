using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Mithril
{
    internal sealed class StringContent : ICellContent
    {
        private String _value;

        public StringContent(String value)
        {
            _value = value;
        }

        public void Write(FileStream decOutput, BinaryWriter decBw)
        {
            if (String.IsNullOrEmpty(_value))
            {
                decBw.Write((Int32)0);
            }
            else
            {
                Encoding encoder = Encoding.UTF8;
                Int32 byteCount = encoder.GetByteCount(_value) + 1; // \0

                Int32 delta = byteCount % 4;
                if (delta != 0)
                    byteCount += (4 - delta);

                Byte[] data = new Byte[byteCount];
                encoder.GetBytes(_value, 0, _value.Length, data, 0);

                decOutput.Write(data, 0, data.Length);
            }
        }

        public void Write(CsvWriter cw)
        {
            String str = _value.Replace("\r\n", "{NewLine}");
            cw.String(str);
        }

        public void ParseValue(String value)
        {
            _value = value.Replace("{NewLine}", "\r\n");
        }
    }
}