using System;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Reflection;

namespace AoC2024
{
    static class Program
    {
        const bool BENCHMARK = false;
        private const int DAY = 1;
        
        public static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            if (BENCHMARK) 
                Benchmark();
            else 
                RunDay(DAY);
            
            stopwatch.Stop();
            TimeSpan stopwatchElapsed = stopwatch.Elapsed;
            Console.WriteLine($"Completed in: {Convert.ToInt32(stopwatchElapsed.TotalMilliseconds)}ms");
        }

        static void RunDay(int day)
        {
            // REFLECTIOOON
            Type type = Type.GetType($"AoC2024.Day{day}");
            MethodInfo method = type.GetMethod("Run");
            var obj = Activator.CreateInstance(type);
            method.Invoke(obj, new object[]{File.ReadAllText($"input/day{day}.txt").Trim().Split('\n').ToList()});
        }
        
        static void Benchmark()
        {
            Console.WriteLine("Running full benchmark: ");
            Console.WriteLine("========================");
            for (int i = 1; i <= DAY; i++)
            {
                Console.WriteLine($"=> Day {i}:");
                RunDay(i);
            }
        }
    }
}