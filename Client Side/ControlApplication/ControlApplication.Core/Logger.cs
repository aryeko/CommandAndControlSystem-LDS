using System;

namespace ControlApplication.Core
{
    public static class Logger
    {
        public static void Log(string log, string tag = "")
        {
            Console.WriteLine($"[{DateTime.Now.TimeOfDay.ToString("g")}]{(!string.IsNullOrEmpty(tag) ? $"[{tag}]" : "")} {log}");
        }
    }
}