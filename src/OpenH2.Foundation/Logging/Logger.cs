using System;

namespace OpenH2.Foundation.Logging
{
    public static class Logger
    {
        public class Color
        {
            internal ConsoleColor foreground;

            private Color(ConsoleColor c)
            {
                foreground = c;
            }

            public static readonly Color White = new Color(ConsoleColor.White);
            public static readonly Color Red = new Color(ConsoleColor.Red);
            public static readonly Color Magenta = new Color(ConsoleColor.Magenta);
            public static readonly Color Cyan = new Color(ConsoleColor.Cyan);
        }

        public static void LogInfo(string message)
        {
            Console.WriteLine(message);
        }

        public static void Log(string message, Color color)
        {
            var c = Console.ForegroundColor;
            Console.ForegroundColor = color.foreground;
            Console.WriteLine(message);
            Console.ForegroundColor = c;
        }
    }
}
