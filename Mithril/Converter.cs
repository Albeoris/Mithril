using System;
using System.IO;
using System.Text;

namespace Mithril
{
    internal class Converter
    {
        private static readonly Encoding UTF8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true, throwOnInvalidBytes: true);

        public void Convert(String directoryPath)
        {
            foreach (String sourcePath in Directory.EnumerateFiles(directoryPath, "*.csh.dec", SearchOption.AllDirectories))
            {
                if (sourcePath.Contains(@"\message_steam\"))
                {
                    Console.Title = "Converting: " + Path.GetFileName(sourcePath);
                    ConvertFile(sourcePath);
                }
            }
        }

        private static void ConvertFile(String sourcePath)
        {
            using (FileStream input = File.OpenRead(sourcePath))
            using (BinaryReader br = new BinaryReader(input))
            {
                CshHeader header = new CshHeader
                {
                    Type = br.ReadBigUInt32(),
                    ColumnNumber = br.ReadBigUInt32(),
                    RowNumber = br.ReadBigUInt32()
                };

                if (header.Type != 0)
                    throw new NotSupportedException($"Unknown type: {header.Type}");

                CshCell[] cells = new CshCell[header.ColumnNumber * header.RowNumber];
                input.DangerousReadStructs(cells, cells.Length);

                String targetPath = Path.ChangeExtension(sourcePath, ".csv");

                using (StreamWriter sw = new StreamWriter(targetPath, false, UTF8))
                {
                    Int32 cellIndex = 0;
                    for (Int32 r = 0; r < header.RowNumber; r++)
                    {
                        for (Int32 c = 0; c < header.ColumnNumber; c++)
                        {
                            CshCell cell = cells[cellIndex++];
                            ICellContent content = ReadContent(input, cell);
                            content.Write(sw);
                            sw.Write(';');
                        }
                        sw.WriteLine();
                    }
                }
            }
        }

        private static ICellContent ReadContent(FileStream input, CshCell cell)
        {
            BinaryReader br = new BinaryReader(input);

            UInt32 offset = cell.Offset;
            UInt32 flags = cell.Flags;

            if (offset == 0)
            {
                return new Int32Content(flags);
            }

            if (flags == 0x40000001)
            {
                return ReadBinaryContent(br, offset);
            }
            if (flags == 0x80000001)
            {
                return ReadBinaryContent(br, offset);
            }
            else if (flags < 0x0FFF)
            {
                return ReadStringContent(input, offset);
            }
            else
            {
                throw new NotSupportedException($"Flags: {flags}");
            }
        }

        private static ICellContent ReadBinaryContent(BinaryReader input, UInt32 offset)
        {
            if (input.BaseStream.Position != offset)
                input.BaseStream.Position = offset;

            return new HexInt32Content(input.ReadBigInt32());
        }

        private static ICellContent ReadStringContent(FileStream input, UInt32 offset)
        {
            if (input.Position != offset)
                input.Position = offset;

            using (MemoryStream ms = new MemoryStream(32))
            {
                Int32 buffSize = 4;
                Byte[] buff = new Byte[buffSize];

                while (true)
                {
                    input.EnsureRead(buff, 0, buffSize);

                    if (buff[buffSize - 1] == 0)
                    {
                        for (int i = 0; i < buffSize - 1; i++)
                        {
                            var value = buff[i];
                            if (value == 0)
                                break;
                            ms.WriteByte(value);
                        }
                        break;
                    }

                    ms.Write(buff, 0, buffSize);
                }

                if (ms.Position == 0)
                    return new StringContent(String.Empty);

                Byte[] result = ms.GetBuffer();
                String str = UTF8.GetString(result, 0, (Int32)ms.Length);

                return new StringContent(str);
            }
        }
    }
}