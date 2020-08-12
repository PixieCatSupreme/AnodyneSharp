using System;

namespace AnodyneSharp.Windows
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using AnodyneGame game = new AnodyneGame();
            game.Run();
        }
    }
}
