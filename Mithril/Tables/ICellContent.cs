using System.IO;

namespace Mithril
{
    interface ICellContent
    {
        void Write(FileStream decOutput, BinaryWriter decBw);
        void Write(CsvWriter cw);
    }
}