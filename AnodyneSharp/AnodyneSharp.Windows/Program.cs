using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AnodyneSharp.Logging;
using AnodyneSharp.Resources;

namespace AnodyneSharp.Windows
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            ResourceManager.GetDirectories = GetDirectories;
            ResourceManager.GetFiles = GetFiles;

            using AnodyneGame game = new AnodyneGame();
            game.Run();
        }

        public static DirectoryInfo[] GetDirectories(string fullPath)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fullPath);

            DirectoryInfo dir = new(path);

            if (!dir.Exists)
            {
                DebugLogger.AddCritical($"Tried loading from {dir.FullName} but failed!", false);
                return Array.Empty<DirectoryInfo>();
            }

            return dir.GetDirectories();
        }

        public static List<FileInfo> GetFiles(string fullPath)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fullPath);

            DirectoryInfo dir = new(path);

            if (!dir.Exists)
            {
                DebugLogger.AddCritical($"Tried loading from {dir.FullName} but failed!", false);
                return new List<FileInfo>();
            }

            return dir.GetFiles().ToList();
        }
    }
}
