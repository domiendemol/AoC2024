namespace AoC2024;

public class Day12
{
    struct Region
    {
        public char plant;
        public List<Vector2Int> positions;
    }
    
    public void Run(List<string> input)
    {
        char[,] inputGrid = Utils.ToCharArray(input);
        
        // build regions
        List<Region> regions = new List<Region>(); // map of positions per plant
        for (int i = 0; i < inputGrid.GetLength(0); i++) {
            for (int j = 0; j < inputGrid.GetLength(1); j++) {
                AddToRegion(inputGrid[i,j], new Vector2Int(i,j), regions, inputGrid);
            }
        }

        Console.WriteLine($"Part 1: {regions.Sum(r => GetPrice(r))}");
        Console.WriteLine($"Part 2: {regions.Sum(r => GetPrice2NewNew(r))}");
        // Console.WriteLine($"Part 2: {regions.Sum(r => GetPrice2New(r))}"); // works for all test cases, not for real input. GRR
    }

    private void AddToRegion(char plant, Vector2Int pos, List<Region> regions, char[,] inputGrid)
    {
        if (inputGrid.TryGetValue(pos.x, pos.y, '\0') == '-') return;
        
        // check if adjacent to an existing region
        // if so, add to it. If not, create new
        var neighbours = GetNeighbours(pos, regions, inputGrid);
        for (int i = -1; i <= 1; i++) {
            for (int j = -1; j <= 1; j++)
            {
                if ((i ==0 && j ==0) || (i != 0 && j != 0)) continue;
                if (inputGrid.TryGetValue(pos.x + i, pos.y + j, '\0') == '\0') continue;
                Region region = regions.FirstOrDefault(r => r.plant == plant && r.positions.Contains(pos + new Vector2Int(i, j)), new Region {plant = '\0'});
                if (region.plant != '\0') {
                    if (!region.positions.Contains(pos))
                    {
                        region.positions.Add(pos);
                        neighbours.Where(n => inputGrid[n.x, n.y] == plant).ToList().ForEach(n => AddToRegion(inputGrid[n.x,n.y], n, regions, inputGrid));
                    }
                    return;
                }
            }
        }

        // region not found yet, create new and try to add visitors
        inputGrid[pos.x, pos.y] = '-'; // mark visited
        regions.Add(new Region{plant = plant, positions = new List<Vector2Int> {pos}});
        // Console.WriteLine($"New region {plant}/{pos}");
        neighbours.ForEach(n => AddToRegion(inputGrid[n.x,n.y], n, regions, inputGrid));
    }

    private List<Vector2Int> GetNeighbours(Vector2Int pos, List<Region> regions, char[,] inputGrid)
    {
        List<Vector2Int> neighbours = new List<Vector2Int>();
        for (int i = -1; i <= 1; i++) { 
            for (int j = -1; j <= 1; j++)
            {
                if ((i == 0 && j == 0) || (i != 0 && j != 0)) continue;
                if (inputGrid.TryGetValue(pos.x + i, pos.y + j, '\0') != inputGrid[pos.x, pos.y]) continue;
                neighbours.Add(pos + new Vector2Int(i, j));
            }
        }
        return neighbours;
    }

    private int GetPrice(Region region)
    {
        // For perimeter: loop every position in region and count nr of non-same-region plants
        int total = 0;
        total += region.positions.Count(i => !region.positions.Contains(new Vector2Int(i.x, i.y + 1)));
        total += region.positions.Count(i => !region.positions.Contains(new Vector2Int(i.x, i.y - 1)));
        total += region.positions.Count(i => !region.positions.Contains(new Vector2Int(i.x + 1, i.y)));
        total += region.positions.Count(i => !region.positions.Contains(new Vector2Int(i.x - 1, i.y)));
        
        // Console.WriteLine($"A region of {region.plant} plants with price {region.positions.Count} * {total} = {total * region.positions.Count}");
        return total * region.positions.Count;
    }

    private int GetPrice2NewNew(Region region)
    {
        // find edge locations, per location/direction
        Dictionary<(int loc, Vector2Int dir), List<Vector2Int>> edges =
            new Dictionary<(int loc, Vector2Int dir), List<Vector2Int>>();
        region.positions.Where(i => !region.positions.Contains(new Vector2Int(i.x + 1, i.y)))
            .ToList().ForEach(i => {
                if (!edges.ContainsKey((i.x+1, new Vector2Int(0, 1)))) edges[(i.x+1, new Vector2Int(0, 1))] = new List<Vector2Int>();
                edges[(i.x+1, new Vector2Int(0, 1))].Add(i);
            });
        region.positions.Where(i => !region.positions.Contains(new Vector2Int(i.x - 1, i.y)))
            .ToList().ForEach(i => {
                if (!edges.ContainsKey((i.x-1, new Vector2Int(0, 1)))) edges[(i.x-1, new Vector2Int(0, 1))] = new List<Vector2Int>();
                edges[(i.x-1, new Vector2Int(0, 1))].Add(i);
            });
        region.positions.Where(i => !region.positions.Contains(new Vector2Int(i.x, i.y + 1)))
            .ToList().ForEach(i => {
                if (!edges.ContainsKey((i.y+1, new Vector2Int(1, 0)))) edges[(i.y+1, new Vector2Int(1, 0))] = new List<Vector2Int>();
                edges[(i.y+1, new Vector2Int(1, 0))].Add(i);
            });
        region.positions.Where(i => !region.positions.Contains(new Vector2Int(i.x, i.y - 1)))
            .ToList().ForEach(i => {
                if (!edges.ContainsKey((i.y-1, new Vector2Int(1, 0)))) edges[(i.y-1, new Vector2Int(1, 0))] = new List<Vector2Int>();
                edges[(i.y-1, new Vector2Int(1, 0))].Add(i);
            });

        // count edges by looping the positions and counting when it's interrupted
        int count = 0;
        foreach (var kvp in edges)
        {
            List<Vector2Int> edgePlaces;
            if (kvp.Key.dir == new Vector2Int(0, 1))
                edgePlaces = kvp.Value.Distinct().OrderBy(v => v.x).ThenBy(v => v.y).ToList();
            else
                edgePlaces = kvp.Value.Distinct().OrderBy(v => v.y).ThenBy(v => v.x).ToList();
            
            count++;
            Vector2Int prev = edgePlaces[0];
            for (int i = 1; i < edgePlaces.Count; i++)
            {
                if (prev + kvp.Key.dir != edgePlaces[i]) count++;
                prev = edgePlaces[i];
            }
        }
        
        // Console.WriteLine($"A region of {region.plant} plants with price {region.positions.Count} * {count} = {count * region.positions.Count}");
        return region.positions.Count * count;
    }
    
    private int GetPrice2New(Region region)
    {
        // For perimeter: loop every position in region and count nr of non-same-region plants
        List<Vector2Int> adjacents = new List<Vector2Int>();
        adjacents = adjacents.Concat(region.positions.Where(i => !region.positions.Contains(new Vector2Int(i.x, i.y + 1))).Select(i => new Vector2Int(i.x, i.y + 1))).ToList();
        adjacents = adjacents.Concat(region.positions.Where(i => !region.positions.Contains(new Vector2Int(i.x, i.y - 1))).Select(i => new Vector2Int(i.x, i.y - 1))).ToList();
        adjacents = adjacents.Concat(region.positions.Where(i => !region.positions.Contains(new Vector2Int(i.x + 1, i.y))).Select(i => new Vector2Int(i.x + 1, i.y))).ToList();
        adjacents = adjacents.Concat(region.positions.Where(i => !region.positions.Contains(new Vector2Int(i.x - 1, i.y))).Select(i => new Vector2Int(i.x - 1, i.y))).ToList();
        
        Dictionary<(Vector2Int pos, Vector2Int dir), bool> removed = new Dictionary<(Vector2Int pos, Vector2Int dir), bool>();
        for (int i = 0; i < adjacents.Count; i++)
        {
            Vector2Int pos = adjacents[i];
            // if (removed.Contains(pos)) continue;
            int removedCount = RemoveLines(pos, adjacents, removed);
            if (removedCount > 0) i = 0;
        }
        //if (region.plant == 'C') adjacents.ForEach(v => debug[v.x+1, v.y] += 1);
        if (region.plant == 'R') Console.WriteLine(string.Join(" ", adjacents));
        
        Console.WriteLine($"A region of {region.plant} plants with price {region.positions.Count} * {adjacents.Count()} = {adjacents.Count() * region.positions.Count}");
        return adjacents.Count() * region.positions.Count;
    }

    private int RemoveLines(Vector2Int pos, List<Vector2Int> adjacents, Dictionary<(Vector2Int pos, Vector2Int dir), bool> removed)
    {
        // maybe find direction/normal?
        int a = RemoveLine(pos, new Vector2Int(0, 1), adjacents, removed);
        if (a > 0) return a;
        int b = RemoveLine(pos, new Vector2Int(0, -1), adjacents, removed);
        if (b > 0) return b;
        int c = RemoveLine(pos, new Vector2Int(1, 0), adjacents, removed); 
        if (c > 0) return c;
        int d = RemoveLine(pos, new Vector2Int(-1, 0), adjacents, removed);
        return d;
    }

    private int RemoveLine(Vector2Int pos, Vector2Int dir, List<Vector2Int> adjacents, Dictionary<(Vector2Int pos, Vector2Int dir), bool> removed)
    {
        if (removed.ContainsKey((pos, dir.Normalize()))) return 0;
        int count = 0;
        for (Vector2Int next = pos + dir; adjacents.Contains(next) && !removed.ContainsKey((next, dir.Normalize())); next += dir) //&& !removed.Contains(next)
        {
            // Console.WriteLine($"Removing {next} from {pos}");
            adjacents.Remove(next);
            removed[(next, dir.Normalize())] = true;
            count ++;
        }
        return count;
    }
}