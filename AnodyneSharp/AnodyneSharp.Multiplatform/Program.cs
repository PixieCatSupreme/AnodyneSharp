using AnodyneSharp.Logging;
using System;

namespace AnodyneSharp.Multiplatform
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
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
