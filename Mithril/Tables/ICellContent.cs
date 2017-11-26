using System.IO;

namespace Mithril
{
    interface ICellContent
    {
        void Write(StreamWriter sw);
    }
}