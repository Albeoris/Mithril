using System;
using System.IO;
using zlib;

namespace Mithril
{
    internal class Decompressor
    {
        public void Decompress(String directoryPath)
        {
            foreach (String sourcePath in Directory.EnumerateFiles(directoryPath, "*.csh", SearchOption.AllDirectories))
            {
                Console.Title = "Decompressing: " + Path.GetFileName(sourcePath);
                DecompressFile(sourcePath);
            }
        }

        private static void DecompressFile(String sourcePath)
        {
            using (FileStream input = File.OpenRead(sourcePath))
            using (BinaryReader br = new BinaryReader(input))
            {
                String targetPath = sourcePath + ".dec";

                input.Seek(0x80, SeekOrigin.Begin);
                Int32 magicNumber = br.ReadBigInt32();
                Int32 uncompressedSize = br.ReadBigInt32();
                Int32 compressedSize = br.ReadBigInt32();
                Int32 compressionType = br.ReadBigInt32();

                if (magicNumber != 1112099930) // ZLIB
                    throw new InvalidDataException("Invalid magic number.");

                if (compressionType == 0)
                {
                    using (Stream outputFile = File.Create(targetPath))
                    {
                        outputFile.SetLength(uncompressedSize);
                        Copy(uncompressedSize, input, outputFile);
                    }
                }
                else if (compressionType == 7)
                {
                    using (Stream outputFile = File.Create(targetPath))
                    using (ZOutputStream output = new ZOutputStream(outputFile))
                    {
                        outputFile.SetLength(uncompressedSize);
                        Decompress(compressedSize, input, output);
                    }
                }
                else if (compressionType == 12)
                {
                    Console.WriteLine($"Unknown compression type: {compressionType}");
                    Console.WriteLine(sourcePath);
                }
                else
                {
                    throw new InvalidDataException("Invalid compression type.");
                }
            }
        }

        private static void Copy(Int32 uncompressedSize, Stream input, Stream output)
        {
            Int32 reading = Math.Min(64 * 1024, uncompressedSize);
            Byte[] buff = new Byte[reading];
            while (uncompressedSize > 0)
            {
                Int32 readed = input.Read(buff, 0, reading);
                if (readed == 0)
                    throw new EndOfStreamException();

                uncompressedSize -= readed;
                output.Write(buff, 0, readed);
                reading = Math.Min(64 * 1024, uncompressedSize);
            }
        }

        private static void Decompress(Int32 compressedSize, FileStream input, ZOutputStream output)
        {
            Int32 reading = Math.Min(64 * 1024, compressedSize);
            Byte[] buff = new Byte[reading];
            while (compressedSize > 0)
            {
                Int32 readed = input.Read(buff, 0, reading);
                if (readed == 0)
                    throw new EndOfStreamException();

                compressedSize -= readed;
                output.Write(buff, 0, readed);
                reading = Math.Min(64 * 1024, compressedSize);
            }
        }
    }
}