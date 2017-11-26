using System;
using System.IO;

namespace Mithril
{
    internal class Compressor
    {
        public void Compress(String directoryPath)
        {
            foreach (String targetPath in Directory.EnumerateFiles(directoryPath, "*.csh", SearchOption.AllDirectories))
            {
                String sourcePath = targetPath + ".dec";

                Console.Title = "Compressing: " + Path.GetFileName(targetPath);
                CompressFile(sourcePath, targetPath);
            }
        }

        private static void CompressFile(String sourcePath, String targetPath)
        {
            String tmpPath = targetPath + ".tmp";
            using (FileStream input = File.OpenRead(sourcePath))
            using (FileStream oldInput = File.OpenRead(targetPath))
            using (BinaryReader oldBr = new BinaryReader(oldInput))
            using (FileStream tmpOutput = File.Create(tmpPath))
            using (BinaryWriter tmpBw = new BinaryWriter(tmpOutput))
            {
                Byte[] header = new Byte[0x80];
                oldInput.EnsureRead(header, 0, header.Length);
                tmpOutput.Write(header, 0, header.Length);

                Int32 magicNumber = oldBr.ReadBigInt32();
                Int32 uncompressedSize = oldBr.ReadBigInt32();
                Int32 compressedSize = oldBr.ReadBigInt32();
                Int32 compressionType = oldBr.ReadBigInt32();

                if (magicNumber != 1112099930) // ZLIB
                    throw new InvalidDataException("Invalid magic number.");

                uncompressedSize = (Int32)input.Length;
                tmpBw.WriteBig(magicNumber);
                tmpBw.WriteBig(uncompressedSize);
                tmpBw.WriteBig(uncompressedSize);
                tmpBw.WriteBig(0); // No compression

                Copy(uncompressedSize, input, tmpOutput);
            }

            String bakPath = targetPath + ".bak";
            if (!File.Exists(bakPath))
                File.Copy(targetPath, bakPath);

            File.Delete(targetPath);
            File.Copy(tmpPath, targetPath);
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
    }
}