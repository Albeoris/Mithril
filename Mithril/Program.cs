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

                if (args.Contains("-d") || args.Length == 1)
                {
                    Decompressor decompressor = new Decompressor();
                    decompressor.Decompress(directoryPath);
                }

                if (args.Contains("-c") || args.Length == 1)
                {
                    Converter converter = new Converter();
                    converter.Convert(directoryPath);
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
            Console.WriteLine("App.exe \"GamePath\" [-d] [-c]");
            Console.WriteLine("\t-d - Decompress .dec");
            Console.WriteLine("\t-c - Convert .dec to .csv");
        }
    }
}