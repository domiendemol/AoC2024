namespace AoC2024;

public class Day8
{
    struct Antenna
    {
        public char freq;
        public Vector2Int pos;
    }
    
    public (string, string) Run(List<string> input)
    {
        char[,] inputGrid = Utils.ToCharArray(input);
        List<Antenna> antennas = inputGrid.Cast<char>().
            Select((c, i) => new Antenna { freq = c, pos = new Vector2Int(i/inputGrid.GetLength(1), i%inputGrid.GetLength(0)) }).Where(a => a.freq != '.').ToList();

        List<Vector2Int> antinodes = antennas.SelectMany(a => GetAntiNodes(a, antennas)).Where(v => inputGrid.TryGetValue(v.x, v.y) != '\0').Distinct().ToList();
        int part1 = antinodes.Count;
   
        antinodes = antennas.SelectMany(a => GetAntiNodes(a, antennas, true)).Where(v => inputGrid.TryGetValue(v.x, v.y) != '\0').Distinct().ToList();
        int part2 = antinodes.Count;
        
        // antinodes.ForEach(v => inputGrid[v.x, v.y] = '#');
        // Utils.PrintCharArray(inputGrid);

        return (part1.ToString(), part2.ToString());
    }

    private List<Vector2Int> GetAntiNodes(Antenna antenna, List<Antenna> antennas, bool all = false)
    {
        List<Antenna> others = antennas.Where(a => a.freq == antenna.freq && !a.Equals(antenna)).ToList();
        return others.SelectMany(a => GetAntiNodes(a, antenna, all) ).ToList();
    }

    private List<Vector2Int> GetAntiNodes(Antenna a, Antenna b, bool all = false)
    {
        Vector2Int dir = b.pos - a.pos;
        List<Vector2Int> nodes = new List<Vector2Int>();
        int count = all ? 0 : 1; // add antennas as well for part 2
        while (count < 50) { // very simple/brute limit, should check grid bounds instead but this is fast enough
            nodes.Add(a.pos - (count * dir));
            nodes.Add(b.pos + (count++ * dir));
            if (!all) break;
        }

        return nodes;
    }
}