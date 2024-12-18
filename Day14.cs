using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace AoC2024;

public class Day14
{
    class Robot
    {
        public int x, y; // top left
        public int vx, vy; // tiles/s

        public void SimulateSecond()
        {
            x = (x + vx);
            while (x<0) x += _width;
            x = x % _width;
            y = y + vy;
            while (y<0) y += _height;
            y = y % _height;
        }
    }

    private static int _width = 101; 
    private static int _height = 103; 
    
    public void Run(List<string> input)
    {
        List<Robot> robots = GetRobots(input);
        if (robots[0].x != 99) {
            _width = 11; _height = 7; // test mode 
        }
        
        // PART 1
        Enumerable.Range(0, 100).ToList().ForEach(i => robots.ForEach(robot => robot.SimulateSecond()));
        Console.WriteLine($"Part 1: {GetSafetyFactor(robots)}");

        // PART 2
        robots = GetRobots(input);
        long sf, prevSf = 0;
        int s = 1;
        for (bool tree = false; !tree; s++)
        {
            robots.ForEach(robot => robot.SimulateSecond());

            sf = GetSafetyFactor(robots);
            if (sf < prevSf * 0.3f) {
                // sudden drop in safety factor for a tree image compared to more scattered layouts
                // figured out by trial/error (lowering limit every time) and printing trees in console
                // after many attempts to use different methods
                // PrintTree(robots);
                tree = true;
            }
            prevSf = GetSafetyFactor(robots);
        }
        Console.WriteLine($"Part 2: {s-1}");
    }
    
    private void PrintTree(List<Robot> robots)
    {       
        for (int y = 0; y <_height; y++) {
            for (int x = 0; x < _width; x++) {
                int count = robots.Count(r => r.x == x && r.y == y);
                Console.Write(count == 0 ? "." : "#");
            }
            Console.WriteLine();
        }
    }

    private List<Robot> GetRobots(List<string> input)
    {
        var numbers = input.Where(line => line.Length > 0).SelectMany(line => Regex.Matches(line, @"(\-*\d+)")).ToList();
        List<Robot> robots = new List<Robot>();
        for (int i = 0; i < numbers.Count(); i+=4)
        {
            robots.Add(new Robot() {
                x = Convert.ToInt32(numbers[i].Value),
                y = Convert.ToInt32(numbers[i+1].Value),
                vx = Convert.ToInt32(numbers[i+2].Value),
                vy = Convert.ToInt32(numbers[i+3].Value),
            });
        }

        return robots;
    }
    
    private long GetSafetyFactor(List<Robot> robots)
    {
        long nrRobots = robots.Count(r => r.x < _width / 2 && r.y < _height / 2);
        nrRobots *= robots.Count(r => r.x > _width / 2 && r.y < _height / 2);
        nrRobots *= robots.Count(r => r.x < _width / 2 && r.y > _height / 2);
        nrRobots *= robots.Count(r => r.x > _width / 2 && r.y > _height / 2);
        return nrRobots;
    }
}