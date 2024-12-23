using System.Diagnostics;
using System.Reflection;

namespace AoC2024
{
    static class Program
    {
        private const string BENCHMARK = "BENCHMARK";
        private const int DAY = 23;
        
        public static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            string arg = args.Length > 0 ? args[0] : "";
            if (arg == BENCHMARK) {
                Benchmark();
            }
            else {
                RunDay(DAY, arg == "TEST");
            }

            stopwatch.Stop();
            TimeSpan stopwatchElapsed = stopwatch.Elapsed;
            Console.WriteLine($"Completed in: {Convert.ToInt32(stopwatchElapsed.TotalMilliseconds)}ms");
        }

        static void RunDay(int day, bool test)
        {
            Console.WriteLine($"=> [Day {day}]");
            
            // REFLECTIOOON
            Type type = Type.GetType($"AoC2024.Day{day}")!;
            MethodInfo? method = type.GetMethod("Run");
            object? obj = Activator.CreateInstance(type);
            string testSuffix = test ? "_test" : "";
            if (!File.Exists($"input/day{day}{testSuffix}.txt")) {
                Console.WriteLine("Error: input file does not exist");
                return;
            }
            method.Invoke(obj, new object[]{File.ReadAllText($"input/day{day}{testSuffix}.txt").Trim().Split('\n').ToList()});
        }
        
        static void Benchmark()
        {
            Console.WriteLine("Running full benchmark: ");
            Console.WriteLine("========================");
            for (int i = 1; i <= DAY; i++)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                
                RunDay(i, false);
                
                stopwatch.Stop();
                TimeSpan stopwatchElapsed = stopwatch.Elapsed;
                Console.WriteLine($"Time: {Convert.ToInt32(stopwatchElapsed.TotalMilliseconds)}ms");
            }
        }
    }
}