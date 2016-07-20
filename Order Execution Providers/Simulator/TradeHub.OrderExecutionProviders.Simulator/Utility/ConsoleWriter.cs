using System;
using System.Runtime.InteropServices;

namespace TradeHub.OrderExecutionProviders.Simulator.Utility
{
    /// <summary>
    /// Provides Calls to Write on Console
    /// </summary>
    public static class ConsoleWriter
    {
        [DllImport("Kernel32.dll")]
        static extern Boolean AllocConsole();

        public static void Write(ConsoleColor color, string value)
        {
            if (!AllocConsole())
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
            if (!AllocConsole())
                WriteLine(color, value, new object[0]);
        }

        public static void WriteLine(ConsoleColor color, String format, params object[] args)
        {
            ConsoleColor tmp = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(format, args);
            Console.ForegroundColor = tmp;
        }

        public static string Prompt()
        {
            if (!AllocConsole())
            {
                Console.Write(">");
                return Console.ReadLine();
            }
            return null;
        }
    }
}
