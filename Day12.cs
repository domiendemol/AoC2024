namespace AoC2024;

public class Day12
{
    struct Region
    {
        public char plant;
        public List<Vector2Int> positions;
    }

    private int[,] debug;
    
    public void Run(List<string> input)
    {
        char[,] inputGrid = Utils.ToCharArray(input);
        debug = new int[inputGrid.GetLength(0), inputGrid.GetLength(1)];
        
        // build regions
        List<Region> regions = new List<Region>(); // map of positions per plant
        for (int i = 0; i < inputGrid.GetLength(0); i++) {
            for (int j = 0; j < inputGrid.GetLength(1); j++) {
                AddToRegion(inputGrid[i,j], new Vector2Int(i,j), regions, inputGrid);
            }
        }

        Console.WriteLine($"Part 1: {regions.Sum(r => GetPrice(r))}");
        Console.WriteLine($"Part 2: {regions.Sum(r => GetPrice2New(r))}");
        
        
        for (int i = 0; i < debug.GetLength(0); i++) {
            for (int j = 0; j < debug.GetLength(1); j++) {
                Console.Write($"{debug[i, j]:0} ");
            }
            Console.WriteLine();
        }
        Console.WriteLine();
        
    }

    private void AddToRegion(char plant, Vector2Int pos, List<Region> regions, char[,] inputGrid)
    {
        if (inputGrid.TryGetValue(pos.x, pos.y, '\0') == '-') return;
        
        // check if adjacent to an existing region
        // if so, add to it
        // if not, create new
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
    
    private int GetPrice2New(Region region)
    {
        // For perimeter: loop every position in region and count nr of non-same-region plants
        List<Vector2Int> adjacents = new List<Vector2Int>();
        adjacents = adjacents.Concat(region.positions.Where(i => !region.positions.Contains(new Vector2Int(i.x, i.y + 1))).Select(i => new Vector2Int(i.x, i.y + 1))).ToList();
        adjacents = adjacents.Concat(region.positions.Where(i => !region.positions.Contains(new Vector2Int(i.x, i.y - 1))).Select(i => new Vector2Int(i.x, i.y - 1))).ToList();
        adjacents = adjacents.Concat(region.positions.Where(i => !region.positions.Contains(new Vector2Int(i.x + 1, i.y))).Select(i => new Vector2Int(i.x + 1, i.y))).ToList();
        adjacents = adjacents.Concat(region.positions.Where(i => !region.positions.Contains(new Vector2Int(i.x - 1, i.y))).Select(i => new Vector2Int(i.x - 1, i.y))).ToList();
        
        // Console.WriteLine(string.Join(", ", adjacents));
        List<Vector2Int> removed = new List<Vector2Int>();
        // go through 
        for (int i = 0; i < adjacents.Count; i++)
        {
            Vector2Int pos = adjacents[i];
            if (removed.Contains(pos)) continue;
            int removedCount = RemoveLines(pos, adjacents, removed);
            i = Math.Max(0, i-removedCount);
        }
        // Console.WriteLine(string.Join(", ", adjacents));
        if (region.plant == 'C') adjacents.ForEach(v => debug[v.x+1, v.y] += 1);

        
        Console.WriteLine($"A region of {region.plant} plants with price {region.positions.Count} * {adjacents.Count()} = {adjacents.Count() * region.positions.Count}");
        return adjacents.Count() * region.positions.Count;
    }

    private int RemoveLines(Vector2Int pos, List<Vector2Int> adjacents, List<Vector2Int> removed)
    {
        int a = RemoveLine(pos, new Vector2Int(0, 1), adjacents, removed);
        if (a > 0) return a;
        int b = RemoveLine(pos, new Vector2Int(0, -1), adjacents, removed);
        if (b > 0) return b;
        int c = RemoveLine(pos, new Vector2Int(1, 0), adjacents, removed); 
        if (c > 0) return c;
        int d = RemoveLine(pos, new Vector2Int(-1, 0), adjacents, removed);
        return d;
    }

    private int RemoveLine(Vector2Int pos, Vector2Int direction, List<Vector2Int> adjacents, List<Vector2Int> removed)
    {
        int count = 0;
        for (Vector2Int next = pos + direction; adjacents.Contains(next); next += direction)
        {
            adjacents.Remove(next);
            removed.Add(next);
            count ++;
        }
        return count;
    }

    private long GetPrice2(Region region)
    {
        long angles = region.positions.Sum(pos => GetInteriorAngles(pos, region));
        Console.WriteLine($"A region of {region.plant} plants with price {region.positions.Count} * {(angles / 180 + 2)} = {region.positions.Count * (angles / 180 + 2)}");
        return region.positions.Count * (angles / 180 + 2);
    }

    private int GetInteriorAngles(Vector2Int pos, Region region)
    {
        // see https://www.sciencing.com/how-to-find-the-number-of-sides-of-a-polygon-12751688/
        // 0 neighbour: 4 angles 90
        // 1 neighbour: 2 angles 90
        // 2 neighbours on same line: 0 angle
        // 2 neighbours not on same line : 1 angle 90 + 1 270
        // 3 neighbors: 1 270
        // 4 neighbour: none
        List<Vector2Int> neighbours = new List<Vector2Int>();
        if (region.positions.Contains(pos + new Vector2Int(0, 1))) neighbours.Add(pos + new Vector2Int(0, 1));
        if (region.positions.Contains(pos + new Vector2Int(0, -1))) neighbours.Add(pos + new Vector2Int(0, -1));
        if (region.positions.Contains(pos + new Vector2Int(1, 0))) neighbours.Add(pos + new Vector2Int(1, 0));
        if (region.positions.Contains(pos + new Vector2Int(-1, 0))) neighbours.Add(pos + new Vector2Int(-1, 0));

        int angles = 0;
        switch (neighbours.Count)
        {
            case 0: 
                angles = 4 * 90; break;
            case 1: 
                angles = 2 * 90; break;
            case 2:
                if (neighbours.All(n => n.x == pos.x) || neighbours.All(n => n.y == pos.y)) angles = 0;
                else {
                    // get the other pos
                    //if (region.positions.Contains(pos + new Vector2Int(1, 1)) ||
                    //    region.positions.Contains(pos + new Vector2Int(1, -1)) ||
                    //    region.positions.Contains(pos + new Vector2Int(-1, 1)) ||
                    //    region.positions.Contains(pos + new Vector2Int(-1, -1)))
                    //        angles = 90;
                    //else angles = 90 + 270;
                    angles = (GetOtherPosInSquare(neighbours, region) > 1) ? 90 : 90 + 270;
                }
                break;
            case 3: 
                //if (region.positions.Contains(pos + new Vector2Int(1, 1)) ||
                //    region.positions.Contains(pos + new Vector2Int(1, -1)) ||
                //    region.positions.Contains(pos + new Vector2Int(-1, 1)) ||
                //    region.positions.Contains(pos + new Vector2Int(-1, -1)))
                //    angles = 0;
                //else angles = 270; break;
                angles = (3 - GetOtherPosInSquare(neighbours, region)) * 270;
                break;
            case 4: 
                angles = 0; break;
        }
        debug[pos.x, pos.y] = angles;
        return angles;
    }

    private int GetOtherPosInSquare(List<Vector2Int> neighbours, Region region)
    {
        int count = 0;
        int xMin = neighbours.Min(v => v.x);
        int xMax = neighbours.Max(v => v.x);
        int yMin = neighbours.Min(v => v.y);
        int yMax = neighbours.Max(v => v.y);
        for (int i = xMin; i <= xMax; i++)
        {
            for (int j = yMin; j <= yMax; j++)
            {
                if (!neighbours.Contains(new Vector2Int(i, j)) && region.positions.Contains(new Vector2Int(i, j))) count++;
                Console.WriteLine($"{i}-{j} -- {count}");
            }
        }
        Console.WriteLine($"{count}");
        return count;
    }
}