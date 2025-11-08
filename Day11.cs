namespace AoC2024;

public class Day11
{
    public (string, string) Run(List<string> input)
    {
        List<long> stones = input[0].Split(' ').Select(c => Convert.ToInt64(c)).ToList();
        Dictionary<(long nr,  int blinks), long> cache = new Dictionary<(long, int), long>();
        
        // PART 1 - normal run, but cache intermediate results for every original stone and blinks/depth
        // could now be replaced with: stones.Sum(st => BlinkCached(st, 25, cache))
        foreach (long stone in stones)  {
            List<long> tStones = new List<long>(){stone};
            for (int i = 0; i < 25; i++) {
                tStones = tStones.SelectMany(st => Blink(st)).ToList();
                cache[(stone, i + 1)] = tStones.Count;
            }        
        }
        
        return (stones.Sum(stone => cache[(stone, 25)]).ToString(), 
                stones.Sum(st => BlinkCached(st, 75, cache)).ToString());
    }

    // returns resulting stone count
    private long BlinkCached(long stone, int blinks, Dictionary<(long, int), long> cache)
    {
        if (blinks == 0) return 1;
        if (cache.ContainsKey((stone, blinks))) return cache[(stone, blinks)];

        List<long> newStones = Blink(stone);
        long result = newStones.Select(st => BlinkCached(st, blinks-1, cache)).Sum();
        cache[(stone, blinks)] = result;
        return result;
    }
    
    // returns resulting stones
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