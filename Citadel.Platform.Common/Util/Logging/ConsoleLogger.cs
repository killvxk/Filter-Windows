using System;

namespace Citadel.Platform.Common.Util
{
    public class ConsoleLogger : IAppLogger
    {
        public ConsoleLogger()
        {
        }

        public void Error(string message)
        {
            Console.WriteLine($"ERROR: {message}");
        }

        public void Error(Exception exception)
        {
            Console.WriteLine($"ERROR: {exception}");
        }

        public void Error(string message, params object[] args)
        {
            Console.WriteLine($"ERROR: {message}", args);
        }

        public void Warn(string message)
        {
            Console.WriteLine($"WARN: {message}");
        }

        public void Info(string message)
        {
            Console.WriteLine($"INFO: {message}");
        }

        public void Info(string message, params object[] args)
        {
            Console.WriteLine($"INFO: {message}", args);
        }

        public void Debug(string message)
        {
            Console.WriteLine($"DEBUG: {message}");
        }

        public void Debug(string message, params object[] args)
        {
            Console.WriteLine($"DEBUG: {message}", args);
        }
    }
}
