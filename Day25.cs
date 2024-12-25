namespace AoC2024;

public class Day25
{
    List<int[]> _locks = new List<int[]>();
    List<int[]> _keys = new List<int[]>();
    
    public void Run(List<string> input)
    {
        for (var i = 0; i < input.Count - 1; i+=8) 
            Parse(input.Skip(i).Take(7).ToList());

        int matches = _locks.Sum(l => _keys.Count(k => IsMatch(l, k)));
        Console.WriteLine($"Part 1: {matches}");
    }

    private bool IsMatch(int[] lockk, int[] key)
    {
        for (int i = 0; i < lockk.Length; i++) {
            if (lockk[i] + key[i] > 5) return false;
        }
        return true;
    }

    private void Parse(List<string> input)
    {
        bool key = input[6].Equals("#####");
        input.RemoveAt(6);
        input.RemoveAt(0);
        
        int[] pinHeights = new int[input[0].Length];
        foreach (string line in input) { 
            for (int i = 0; i < line.Length; i++) {
                if (line[i] == '#') pinHeights[i]++;
            }
        }

        if (key) _keys.Add(pinHeights);
        else _locks.Add(pinHeights);
    }
}