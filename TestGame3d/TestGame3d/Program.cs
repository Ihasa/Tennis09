using System;

namespace Tennis01
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>

        static void Main(string[] args)
        {
            using (GameMain game = new GameMain("Sweet Spot!"))
            {
                game.Run();
            }
        }
    }
#endif
}

