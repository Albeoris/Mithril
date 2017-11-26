using System;
using System.Linq;

namespace Mithril
{
    static class Program
    {
        static void Main(String[] args)
        {
            try
            {
                if (args.Length < 1)
                {
                    ShowHelp();
                    Console.WriteLine();
                    return;
                }

                String directoryPath = args[0];

                if (args.Contains("-r") || args.Length == 1)
                {
                    Restorer restorer = new Restorer();
                    restorer.Restore(directoryPath);
                }

                if (args.Contains("-d") || args.Length == 1)
                {
                    Decompressor decompressor = new Decompressor();
                    decompressor.Decompress(directoryPath);
                }

                if (args.Contains("-tf") || args.Length == 1)
                {
                    Transformer transformer = new Transformer();
                    transformer.TransformForward(directoryPath);
                }

                if (args.Contains("-tb") || args.Length == 1)
                {
                    Transformer transformer = new Transformer();
                    transformer.TransformBack(directoryPath);
                }

                if (args.Contains("-c") || args.Length == 1)
                {
                    Compressor compressor = new Compressor();
                    compressor.Compress(directoryPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("------------------------");
                Console.WriteLine(ex);
                Console.WriteLine("------------------------");
            }
            finally
            {
                Console.WriteLine("Press Enter to exit...");
                Console.ReadLine();
            }
        }

        private static void ShowHelp()
        {
            Console.WriteLine("Mithril.exe \"GamePath\" [-r] [-d] [-tf] [-tb] [-c]");
            Console.WriteLine("\t-r - Restore .csh from .bak");
            Console.WriteLine("\t-d - Decompress .csh to .dec");
            Console.WriteLine("\t-tf - Transform .dec to .csv");
            Console.WriteLine("\t-tb - Transform .csv to .dec");
            Console.WriteLine("\t-c - Compress .dec to .csh");
        }
    }
}