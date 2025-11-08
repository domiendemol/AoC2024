namespace AoC2024;

public class Day10
{
    public (string, string) Run(List<string> input)
    {
        // int[,] inputGrid = Utils.ToIntArray(input);
        Dictionary<Vector2Int, int> map = input.SelectMany((line, y) => line.Select((h, x) => (pos: new Vector2Int(x, y), height: (int) char.GetNumericValue(h))))
            .ToDictionary(t => t.pos, t => t.height);

        int part1 = map.Keys.Where(k => map[k] == 0).Sum(v => GetTrailHeadsDFS(v, -1, map, new List<Vector2Int>(), new List<Vector2Int>()));
        int part2 = map.Keys.Where(k => map[k] == 0).Sum(v => GetTrailHeadRatingsDFS(v, -1, map, new List<Vector2Int>()));

        return (part1.ToString(), part2.ToString());
    }
    
    private int GetTrailHeadsDFS(Vector2Int pos, int prevHeight, Dictionary<Vector2Int, int> map, List<Vector2Int> visited, List<Vector2Int> trailheads)
    {
        if (!map.ContainsKey(pos)) return 0;
        if (visited.Contains(pos)) return 0;
        if (map[pos] != prevHeight + 1) return 0;
        if (map[pos] == 9) {
            if (trailheads.Contains(pos)) return 0; // each trailhead counts only as 1
            trailheads.Add(pos);
            return 1;
        }

        // check neighbours
        visited.Add(pos);
        int result = GetTrailHeadsDFS(pos+new Vector2Int(0, 1), map[pos], map, visited, trailheads) + GetTrailHeadsDFS(pos+new Vector2Int(1,0), map[pos], map, visited, trailheads) + 
                     GetTrailHeadsDFS(pos+new Vector2Int(-1,0), map[pos], map, visited, trailheads) + GetTrailHeadsDFS(pos+new Vector2Int(0,-1), map[pos], map, visited, trailheads);
        visited.Remove(pos);
        
        return result;
    }
    
    private int GetTrailHeadRatingsDFS(Vector2Int pos, int prevHeight, Dictionary<Vector2Int, int> map, List<Vector2Int> visited)
    {
        if (!map.ContainsKey(pos)) return 0;
        if (visited.Contains(pos)) return 0;
        if (map[pos] != prevHeight + 1) return 0;
        if (map[pos] == 9) return 1;

        // check neighbours
        visited.Add(pos);
        int result = GetTrailHeadRatingsDFS(pos+new Vector2Int(0, 1), map[pos], map, visited) + GetTrailHeadRatingsDFS(pos+new Vector2Int(1,0), map[pos], map, visited) + 
                     GetTrailHeadRatingsDFS(pos+new Vector2Int(-1,0), map[pos], map, visited) + GetTrailHeadRatingsDFS(pos+new Vector2Int(0,-1), map[pos], map, visited);
        visited.Remove(pos);
        
        return result;
    }
}