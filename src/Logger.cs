using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Apps72.Dev.SqlClr
{
    public static partial class Logger
    {
        private const ConsoleColor RED = ConsoleColor.Red;
        private const ConsoleColor GRAY = ConsoleColor.Gray;

        public static string LogFile { get; set; }

        public static void WriteError(string message)
        {
            if (!string.IsNullOrEmpty(LogFile))
                System.IO.File.AppendAllText(LogFile, $"Error: {message}{Environment.NewLine}");

            Console.ResetColor();
            Console.ForegroundColor = RED;
            Console.WriteLine($"Error: {message}");
        }

        public static void WriteInfo(string message)
        {
            if (!string.IsNullOrEmpty(LogFile))
                System.IO.File.AppendAllText(LogFile, $"{message}{Environment.NewLine}");

            Console.ResetColor();
            Console.WriteLine($"{message}");
        }

        public static void WriteHelp(string message)
        {
            Console.ResetColor();
            Console.WriteLine(message);
        }
  
    }
}
