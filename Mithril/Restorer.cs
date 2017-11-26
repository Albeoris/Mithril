using System;
using System.IO;

namespace Mithril
{
    internal class Restorer
    {
        public void Restore(String directoryPath)
        {
            foreach (String sourcePath in Directory.EnumerateFiles(directoryPath, "*.csh.bak", SearchOption.AllDirectories))
            {
                Console.Title = "Restoring: " + Path.GetFileName(sourcePath);
                String targetPath = sourcePath.Substring(0, sourcePath.Length - 4);
                File.Copy(sourcePath, targetPath, true);
            }
        }
    }
}