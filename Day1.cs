using System.Text.RegularExpressions;

namespace AoC2024;

public class Day1
{
    public void Run(List<string> lines)
    {
        // parse input: read into 2 lists
        List<int> left = lines.Select(line => Int32.Parse(Regex.Match(line, @"([0-9]+) ").Groups[1].Value)).ToList();
        List<int> right = lines.Select(line => Int32.Parse(Regex.Match(line, @" ([0-9]+)").Groups[1].Value)).ToList();

        Part1(left, right);
        Part2(left, right);
    }

    void Part1(List<int> left, List<int> right)
    {
        left.Sort();
        right.Sort();

        long total = 0;
        for (int i = 0; i < left.Count; i++) {
            total += Math.Abs(left[i] - right[i]);
        }
        Console.WriteLine($"PART 1: {total}");
    }
    
    void Part2(List<int> left, List<int> right)
    {
        long total = left.Sum(l => right.Sum(r => (l == r) ? l : 0));
        Console.WriteLine($"PART 2: {total}");           
    }
}