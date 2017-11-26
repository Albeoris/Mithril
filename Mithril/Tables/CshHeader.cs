namespace Mithril
{
    public struct CshHeader
    {
        public BigEndianUInt32 Type;
        public BigEndianUInt32 ColumnNumber;
        public BigEndianUInt32 RowNumber;
    }
}