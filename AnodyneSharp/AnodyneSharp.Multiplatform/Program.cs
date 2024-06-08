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
                ResourceManager.BaseDir = AppDomain.CurrentDomain.BaseDirectory;

                using AnodyneGame game = new AnodyneGame();
                game.Run();
            }
            catch (Exception ex)
            {
                DebugLogger.AddException(ex);
            }

        }
    }
}
