using System;
using System.IO;
using System.Text;
using Mithril;

namespace Mithril
{
    internal class Transformer
    {
        private static readonly Encoding UTF8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true, throwOnInvalidBytes: true);

        public void TransformForward(String directoryPath)
        {
            foreach (String sourcePath in Directory.EnumerateFiles(directoryPath, "*.csh.dec", SearchOption.AllDirectories))
            {
                if (sourcePath.Contains(@"\message_steam\"))
                {
                    Console.Title = "Transform to CSV: " + Path.GetFileName(sourcePath);
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

                ICellContent[] buff = new ICellContent[header.ColumnNumber];

                using (CsvWriter writer = new CsvWriter(targetPath))
                {
                    Int32 cellIndex = 0;
                    for (Int32 r = 0; r < header.RowNumber; r++)
                    {
                        for (Int32 c = 0; c < header.ColumnNumber; c++)
                        {
                            CshCell cell = cells[cellIndex++];
                            ICellContent content = ReadContent(input, cell);
                            buff[c] = content;
                        }

                        ContentEntry entry = new ContentEntry(buff);
                        writer.WriteEntry(entry, null);
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
                return new Int32ContentWithoutOffset(flags);
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
                        for (Int32 i = 0; i < buffSize - 1; i++)
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

        public void TransformBack(String directoryPath)
        {
            foreach (String csvPath in Directory.EnumerateFiles(directoryPath, "*.csh.csv", SearchOption.AllDirectories))
            {
                String decPath = Path.ChangeExtension(csvPath, ".dec");
                if (!File.Exists(decPath))
                {
                    Console.WriteLine("Cannot find meta file: {0}", decPath);
                    continue;
                }

                Console.Title = "Transform from CSV: " + Path.GetFileName(csvPath);
                TransformBackFile(csvPath, decPath);
            }
        }

        private static void TransformBackFile(String csvPath, String decPath)
        {
            String tmpPath;

            CshCell[] cells;
            using (FileStream decInput = File.OpenRead(decPath))
            using (BinaryReader decBr = new BinaryReader(decInput))
            {
                CshHeader header = new CshHeader
                {
                    Type = decBr.ReadBigUInt32(),
                    ColumnNumber = decBr.ReadBigUInt32(),
                    RowNumber = decBr.ReadBigUInt32()
                };

                if (header.Type != 0)
                    throw new NotSupportedException($"Unknown type: {header.Type}");

                cells = new CshCell[header.ColumnNumber * header.RowNumber];
                decInput.DangerousReadStructs(cells, cells.Length);

                RowCsvEntry[] data = CsvReader.Read<RowCsvEntry>(csvPath);
                if (header.RowNumber != data.Length)
                    throw new InvalidDataException($"header.RowNumber ({header.RowNumber}) != data.Length ({data.Length})");

                tmpPath = decPath + ".tmp";
                using (FileStream decOutput = File.Create(tmpPath))
                using (BinaryWriter decBw = new BinaryWriter(decOutput))
                {
                    decBw.WriteBig(header.Type);
                    decBw.WriteBig(header.ColumnNumber);
                    decBw.WriteBig(header.RowNumber);
                    Int64 headerOffset = decOutput.Position;

                    // Reserve space for header
                    decOutput.DangerousWriteStructs(cells, cells.Length);

                    if (decOutput.Position != decInput.Position)
                        throw new InvalidDataException();

                    AlignData(decOutput);

                    Int32 cellIndex = 0;
                    for (Int32 r = 0; r < header.RowNumber; r++)
                    {
                        RowCsvEntry row = data[r];

                        for (Int32 c = 0; c < header.ColumnNumber; c++)
                        {
                            CshCell cell = cells[cellIndex++];
                            ICellContent cellContent = ReadContent(decInput, cell);

                            if (cell.Offset != 0)
                            {
                                cell.Offset = (UInt32)decOutput.Position;
                                cells[cellIndex - 1] = cell; // It's a struct
                            }

                            if (cellContent is StringContent str)
                                str.ParseValue(CsvParser.String(row.Raw[c]));

                            cellContent.Write(decOutput, decBw);
                            AlignData(decOutput);
                        }
                    }

                    // Update offsets
                    decOutput.Position = headerOffset;
                    decOutput.DangerousWriteStructs(cells, cells.Length);


                    // For test
                    //CheckFiles(decInput, decOutput);
                }
            }

            File.Delete(decPath);
            File.Move(tmpPath, decPath);
        }

        private static void CheckFiles(FileStream decInput, FileStream decOutput)
        {
            Int32 size = (Int32)decInput.Length;
            if (size != decOutput.Length)
                throw new InvalidDataException();

            decInput.Position = 0;
            decOutput.Position = 0;

            Int32 buffSize = Math.Min(64 * 1024, size);
            Byte[] buff1 = new Byte[buffSize];
            Byte[] buff2 = new Byte[buffSize];

            while (size > 0)
            {
                Int32 toRead = Math.Min(size, buffSize);
                Int32 readed = decInput.Read(buff1, 0, toRead);
                Int32 r2 = decOutput.Read(buff2, 0, toRead);

                if (readed != r2)
                    throw new InvalidDataException();

                size -= readed;
                if (readed == 0)
                    throw new EndOfStreamException();

                for (Int32 i = 0; i < readed; i++)
                    if (buff1[i] != buff2[i])
                        throw new InvalidDataException();
            }
        }

        private static void AlignData(FileStream decOutput)
        {
            Int64 delta = decOutput.Position % 4;
            if (delta != 0)
                decOutput.Position += (4 - delta);
        }
    }

    internal class ContentEntry : ICsvEntry
    {
        private readonly ICellContent[] _buff;

        public ContentEntry(ICellContent[] buff)
        {
            _buff = buff;
        }

        public void ParseEntry(String[] raw)
        {
            throw new NotImplementedException();
        }

        public void WriteEntry(CsvWriter sw)
        {
            foreach (var content in _buff)
                content.Write(sw);
        }
    }

    internal sealed class RowCsvEntry : ICsvEntry
    {
        public String[] Raw;

        public void ParseEntry(String[] raw)
        {
            Raw = raw;
        }

        public void WriteEntry(CsvWriter sw)
        {
            throw new NotImplementedException();
        }
    }
}