using System;

namespace Mithril
{
    public interface ICsvEntry
    {
        void ParseEntry(String[] raw);
        void WriteEntry(CsvWriter sw);
    }
}