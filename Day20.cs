using System.Reflection.Metadata;

namespace AoC2024;

public class Day20
{	
    private static Vector2Int[] _directions = new[] { new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(0, -1), new Vector2Int(-1, 0) };
    private List<Node> _nodes = new List<Node>();
    private Dictionary<Vector2Int, Node> _nodeMap = new Dictionary<Vector2Int, Node>(); // for speeding up TryAddSiblings
    private Dictionary<Node, int> _pathLengthFromCache = new Dictionary<Node, int>(); // from node to end, assuming no cheats
    private Dictionary<Node, int> _pathLengthToCache = new Dictionary<Node, int>(); // from start to node, assuming no cheats
    
    public (string, string) Run(List<string> input)
    {
        for (var i = 0; i < input.Count; i++) {
            for (int j = 0; j < input[i].Length; j++)
            {
                Node node = new Node(new Vector2Int(i, j), input[i][j], input[i][j] == '#');
                _nodes.Add(node);
                TryAddSiblings(node, input);
            }
        }
        Node endNode = _nodes.First(node => node.id == 'E');
        
        // Do a normal run to cache all costs
        FindShortestPath(int.MaxValue, endNode);
        CachePaths(endNode);
        int fullPathLength = endNode.cost;

        // Approach 2
        List<(int, Node, Node)> pathLengths = GetUniqueCheatedPaths(fullPathLength, 2);
        int part1 = pathLengths.Count(pl => (fullPathLength - pl.Item1) >= 100);

        // Part 2
        pathLengths = GetUniqueCheatedPaths(fullPathLength, 20);
        int part2 = pathLengths.Count(pl => (fullPathLength - pl.Item1) >= 100);
        
        return (part1.ToString(), part2.ToString());
    }

    // loop nodes on path
    // for every node try every 'cheat end' within cheats distance
    // use cached path lengths to return answer = cost to start of cheat + cost from start of cheat + length of cheat
    private List<(int, Node, Node)> GetUniqueCheatedPaths(int fullPathLength, int cheats)
    {
        List<(int, Node, Node)> pathLengths = new List<(int, Node, Node)>();

        foreach (Node cheatStart in _nodes.Where(n => !n.wall)) 
        {
            List<Node> cheatEnds = new List<Node>();
            AddCheatEnds(cheatStart, cheats, cheatEnds);
            foreach (Node cheatEnd in cheatEnds.Where(node => !node.wall).Distinct())
            {
                int dist = Math.Abs(cheatEnd.pos.x - cheatStart.pos.x) + Math.Abs(cheatEnd.pos.y - cheatStart.pos.y);
                if (_pathLengthToCache[cheatStart] + _pathLengthFromCache[cheatEnd] + dist >= fullPathLength) continue;
                pathLengths.Add((_pathLengthToCache[cheatStart] + _pathLengthFromCache[cheatEnd] + dist, cheatStart, cheatEnd));
            }
        }
        pathLengths = pathLengths.Where(pl => pl.Item1 != fullPathLength).Distinct().ToList();
        // Console.WriteLine($"Unique paths: {pathLengths.Count()}");
        return pathLengths;
    }

    private void AddCheatEnds(Node node, int depth, List<Node> cheatEnds)
    {
        for (int i = -depth; i <= depth; i++)
        {
            for (int j = -depth; j <= depth; j++)
            {
                if (Math.Abs(i)+Math.Abs(j)>depth) continue;
                if (_nodeMap.TryGetValue(node.pos + new Vector2Int(i,j), out Node end) && !node.wall)
                    cheatEnds.Add(end);
            }
        }
    }
    
    private void TryAddSiblings(Node node, List<string> input)
    {
        foreach (Vector2Int dir in _directions)
        {
            if (node.pos.x + dir.x < 0 || node.pos.x + dir.x >= input.Count) continue;
            if (node.pos.y + dir.y < 0 || node.pos.y + dir.y >= input[0].Length) continue;
            char val = Utils.TryGetValue(input[node.pos.x + dir.x].ToCharArray(), node.pos.y + dir.y);
            if (val != '\0') // & val != '#'
            {
                if (_nodeMap.TryGetValue(node.pos + dir, out Node sibling))
                {
                    if (!sibling.siblings.Contains(node)) sibling.siblings.Add(node);
                    if (!node.siblings.Contains(sibling)) node.siblings.Add(sibling);
                }
            }
        }
        _nodeMap.Add(node.pos, node);
    }
    
    // cheat stuff isn't actually used anymore 
    private void FindShortestPath(int maxPathLength, Node end)
    {
        Queue<(Node, int)> queue = new Queue<(Node, int)>();
        
        Node start = _nodes.First(node => node.id == 'S');
        start.cost = 0;
        queue.Enqueue((start, 0));
        
        while (queue.Count > 0)
        {
            (Node, int) nodeT = queue.Dequeue();
            Node node = nodeT.Item1;
            int cheatsUsed = nodeT.Item2;

            if (node.cost > maxPathLength) return; // no need to continue

            if (cheatsUsed == 2 && _pathLengthFromCache.TryGetValue(node, out var value)) {
                end.cost = node.cost + value;
                return;
            }
            
            // check its neighbours
            List<(Node, Vector2Int, int)> possibleSteps = GetPossibleDirections(node, cheatsUsed);
            foreach ((Node node, Vector2Int, int newCheats) nextStep in possibleSteps)
            {
                if (node.cost + 1 <= nextStep.node.cost)
                {
                    if (node.cost + 1 < nextStep.node.cost)
                    {
                        nextStep.node.cost = node.cost + 1;
                        nextStep.node.prevNodes.Add(node);
                        queue.Enqueue((nextStep.node, nextStep.newCheats));
                        if (nextStep.newCheats > cheatsUsed) node.cheatIndex = nextStep.newCheats;
                        if (nextStep.newCheats > cheatsUsed && nextStep.newCheats == 2) nextStep.node.cheatIndex = 3;
                    }
                }
            }
        }
    }

    private List<(Node, Vector2Int dir, int cheats)> GetPossibleDirections(Node node, int cheatsUsed)
    {
        List<(Node next, Vector2Int dir, int)> result = new();
        for (int i = 0; i < _directions.Length; i++)
        {
            Vector2Int nextPos = node.pos + _directions[i];
            Node next = node.siblings.FirstOrDefault(n => n.pos == node.pos + _directions[i], null);
            
            if (next != null)
            {
                // if (cheatsUsed == 1 && !next.wall) continue;
                if (cheatsUsed == 1 && next.wall) continue;
                if (cheatsUsed == 2 && next.wall) continue;
                if (cheatsUsed == 0 && next.wall && !next.cheatWall) continue;
                int newCheats = cheatsUsed;
                if (cheatsUsed == 1) newCheats++;
                if (cheatsUsed == 0 && next.cheatWall) newCheats++;

                // bool cheating = next.cheatWall || node.cheatWall;
                result.Add((next, nextPos, newCheats));
            }
        }

        return result;
    }

    private void PrintMap(List<string> input, Node end)
    {
        List<Node> pathNodes = new List<Node>();
        List<Node> queue = new List<Node>(){end};
        while (queue.Count > 0)
        {
            Node t = queue[0];
            queue.RemoveAt(0);
            foreach (Node prev in t.prevNodes) {
                if (!pathNodes.Contains(prev)) queue.Add(prev);
            }
            pathNodes.AddRange(t.prevNodes);
        }
        for (int i = 0; i < input.Count(); i++)
        {
            for (int j = 0; j < input[i].Length; j++)
            {
                Node n = pathNodes.FirstOrDefault(n => n.pos == new Vector2Int(i, j), null);
                if (n != null) 
                    Console.Write(n.cheatIndex > 0 ? ""+n.cheatIndex : n.id != '.' ? n.id : 'O');
                else
                    Console.Write(input[i][j]);
            }
            Console.WriteLine();
        }
    }
    
    private void CachePaths(Node end)
    {
        List<Node> pathNodes = new List<Node>();
        List<Node> queue = new List<Node>(){end};
        while (queue.Count > 0)
        {
            Node t = queue[0];
            queue.RemoveAt(0);
            
            _pathLengthFromCache.Add(t, end.cost - t.cost);
            _pathLengthToCache.Add(t, t.cost);
            
            foreach (Node prev in t.prevNodes) {
                if (!pathNodes.Contains(prev)) queue.Add(prev);
            }
            pathNodes.AddRange(t.prevNodes);
        }
    }
    
    // runs part 1 in 22 seconds
    private void Approach1(int pathLength, Node endNode)
    {
        List<(int, Node, Node)> pathLengths = new List<(int, Node, Node)>();
        int counter = 0;
        // try every wall as possible 'cheat wall' and run pathfinding
        foreach (Node wall in _nodes.Where(n => n.wall))
        {
            _nodes.ForEach(n => n.Reset()); 
            wall.cheatWall = true;

            FindShortestPath(pathLength - 100, endNode);
            counter ++ ;
            
            int result = endNode.cost;
            pathLengths.Add((result, _nodes.FirstOrDefault(n => n.cheatIndex == 1, null), _nodes.FirstOrDefault(n => n.cheatIndex == 2, null)));
            
            // if (result == 82) PrintMap(input, endNode);
            wall.cheatWall = false;
        }
        
        Console.WriteLine($"Part 1 pathfinding counter: {counter}");

        pathLengths = pathLengths.Where(pl => pl.Item1 != pathLength).ToList();
        pathLengths = pathLengths.Distinct().ToList();
        Console.WriteLine($"Part 1: {pathLengths.Count(pl => (pathLength - pl.Item1) >= 100)}");
    }
    
    class Node : IComparable<Node>
    {
        public char id;
        public Vector2Int pos;
        public List<Node> siblings = new List<Node>();
        public HashSet<Node> prevNodes = new HashSet<Node>();
        // public bool visited;
        public int cost = Int32.MaxValue;
        public bool wall;
        public bool cheatWall;
        public int cheatIndex;
        
        public Node(Vector2Int pos, char id, bool wall)
        {
            this.pos = pos;
            this.id = id;
            this.wall = wall;
        }

        public int CompareTo(Node? other)
        {
            if (other == null) return 1;
            return cost.CompareTo(other.cost);
        }

        public void Reset()
        {
            cost = int.MaxValue;
            prevNodes.Clear();
            cheatIndex = 0;
        }
    }
}