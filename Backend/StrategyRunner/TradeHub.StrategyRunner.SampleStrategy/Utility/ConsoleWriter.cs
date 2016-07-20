using System;
using System.Runtime.InteropServices;

namespace TradeHub.StrategyRunner.SampleStrategy.Utility
{
    /// <summary>
    /// Provides Calls to Write on Console
    /// </summary>
    public static class ConsoleWriter
    {
        public static void Write(ConsoleColor color, string value)
        {
            Write(color, value, new object[0]);
        }

        public static void Write(ConsoleColor color, String format, params object[] args)
        {
            ConsoleColor tmp = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(format, args);
            Console.ForegroundColor = tmp;
        }

        public static void WriteLine(ConsoleColor color, string value)
        {
            WriteLine(color, value, new object[0]);
        }

        public static void WriteLine(ConsoleColor color, String format, params object[] args)
        {
            ConsoleColor tmp = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(format, args);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static string Prompt()
        {
            Console.Write(">");
            return Console.ReadLine();
        }
    }
}
