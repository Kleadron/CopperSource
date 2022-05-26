using System;

namespace KSoft.Client
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            using (Engine game = new Engine())
            {
                game.Run();
            }
        }
    }
#endif
}

