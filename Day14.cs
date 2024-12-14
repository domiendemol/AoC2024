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
            while (x<0) x += _height;
            x = x % _height;
            y = y + vy;
            while (y<0) y += _width;
            y = y % _width;
        }
    }

    private static int _width = 101; //11;
    private static int _height = 103; // // 7;
    
    public void Run(List<string> input)
    {
        var numbers = input.Where(line => line.Length > 0).SelectMany(line => Regex.Matches(line, @"(\-*\d+)")).ToList();
        List<Robot> robots = new List<Robot>();
        for (int i = 0; i < numbers.Count(); i+=4)
        {
            robots.Add(new Robot() {
                y = Convert.ToInt32(numbers[i].Value),
                x = Convert.ToInt32(numbers[i+1].Value),
                vy = Convert.ToInt32(numbers[i+2].Value),
                vx = Convert.ToInt32(numbers[i+3].Value),
            });
        }

        if (robots[0].y != 99) {
            _width = 11;
            _height = 7;
        }
        Console.WriteLine($"Part 1: {robots[0].vx}, {robots[0].vy}");
        Console.WriteLine($"Part 1: {robots[1].vx}, {robots[1].vy}");
        Console.WriteLine($"Part 1: {robots[2].vx}, {robots[2].vy}");
        Console.WriteLine($"Part 1: {robots[3].vx}, {robots[3].vy}");
        Console.WriteLine($"Part 1: {robots[4].vx}, {robots[4].vy}");
        // Console.WriteLine($"Part 1: {robots[0].x}, {robots[0].y}");
       Enumerable.Range(0, 100).ToList().ForEach(i => robots.ForEach(robot => robot.SimulateSecond()));
        Console.WriteLine($"Part 1: {robots[0].x}, {robots[0].y}");
        Console.WriteLine($"Part 1: {robots[1].x}, {robots[1].y}");
        Console.WriteLine($"Part 1: {robots[2].x}, {robots[2].y}");
        Console.WriteLine($"Part 1: {robots[3].x}, {robots[3].y}");
        Console.WriteLine($"Part 1: {robots[4].x}, {robots[4].y}");
        
        Console.WriteLine($"Part 1: {GetSafetyFactor(robots)}");

        for (int x = 0; x < _height; x++)
        {
            for (int y = 0; y < _width; y++)
            {
                Console.Write(robots.Count(r => r.x == x && r.y == y));
            }
            Console.WriteLine();
        }
    }

    
    
    private long GetSafetyFactor(List<Robot> robots)
    {
        long nrRobots = robots.Where(r => r.x < _height / 2 && r.y < _width / 2).Count();
        nrRobots *= robots.Where(r => r.x > _height / 2 && r.y < _width / 2).Count();
        nrRobots *= robots.Where(r => r.x < _height / 2 && r.y > _width / 2).Count();
        nrRobots *= robots.Where(r => r.x > _height / 2 && r.y > _width / 2).Count();
        return nrRobots;
    }
}