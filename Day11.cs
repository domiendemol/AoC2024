namespace AoC2024;

public class Day11
{
    public void Run(List<string> input)
    {
        List<long> stones = input[0].Split(' ').Select(c => Convert.ToInt64(c)).ToList();

        for (int i = 0; i < 25; i++) {
            stones = stones.SelectMany(st => Blink(st)).ToList();
            // Console.WriteLine("==> "); stones.ForEach(st => Console.Write(st + " ")); 
        }
        long part1 = stones.Count;
       
        Console.WriteLine($"Part 1: {part1}");
    }

    private List<long> Blink(List<long> stones)
    {
        List<string> result = new List<string>();
        return stones.SelectMany(stone => Blink(stone)).ToList();
    }
    
    private List<long> Blink(long stone)
    {
        if (stone == 0) return new List<long>() { 1 };
        string stoneStr = Convert.ToString(stone);
        if (stoneStr.Length % 2 == 0)
        {
            return new List<long>(){ Convert.ToInt64(stoneStr.Substring(0, stoneStr.Length / 2)), Convert.ToInt64(stoneStr.Substring(stoneStr.Length / 2)) };
        }

        return new List<long>() { stone * 2024 };
    }
}