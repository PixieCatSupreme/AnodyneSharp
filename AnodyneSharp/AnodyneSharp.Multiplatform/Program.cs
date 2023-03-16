using AnodyneSharp.Logging;
using AnodyneSharp.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AnodyneSharp.Multiplatform
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                ResourceManager.GetDirectories = GetDirectories;
                ResourceManager.GetFiles = GetFiles;

                using AnodyneGame game = new AnodyneGame();
                game.Run();
            }
            catch (Exception ex)
            {
                DebugLogger.AddException(ex);
            }

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
