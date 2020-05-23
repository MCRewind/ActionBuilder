using System;

namespace ActionBuilder
{
    public static class Program
    {
        [STAThread]
        private static void Main()
        {
            using var game = new ActionBuilder();
            game.Run();
        }
    }
}