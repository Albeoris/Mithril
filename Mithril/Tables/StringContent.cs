using System;
using System.IO;
using System.Linq;

namespace Mithril
{
    internal class StringContent : ICellContent
    {
        private readonly String _str;

        public StringContent(String str)
        {
            _str = str;
        }

        public void Write(StreamWriter sw)
        {
            if (_str.Contains(';'))
            {
                sw.Write('"');
                WriteStr(sw);
                sw.Write('"');
            }
            else
            {
                WriteStr(sw);
            }
        }

        private void WriteStr(StreamWriter sw)
        {
            String str = _str.Replace("\"", "\"\"");
            sw.Write(str);
        }
    }
}