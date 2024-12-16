namespace AoC2024;

public class Day16
{	
    private static Vector2Int[] _directions = new[] { new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(0, -1), new Vector2Int(-1, 0) };
    private List<Node> _nodes = new List<Node>();
    private Dictionary<Vector2Int, Node> _nodeMap = new Dictionary<Vector2Int, Node>(); // for speeding up TryAddSiblings
    
    public void Run(List<string> input)
    {
        for (var i = 0; i < input.Count; i++) {
            for (int j = 0; j < input[i].Length; j++)
            {
                if (input[i][j] == '#') continue;
                Node node = new Node(new Vector2Int(i, j), input[i][j]);
                _nodes.Add(node);
                TryAddSiblings(node, input);
            }
        }
        
        // Part 1
        FindShortestPath();
        Console.WriteLine($"Part 1: {_nodes.First(node => node.id == 'E').cost}");
        
        // Part 2
        // we stored the previousNodes
        // so go back from E to S, follow all paths
        HashSet<Node> bestNodes = new HashSet<Node>(){};
        Node start = _nodes.First(node => node.id == 'E');
        bestNodes.Add(start);
        AddBestPathNodes(start, bestNodes);
        Console.WriteLine($"Part 2: {bestNodes.Count+1}");
        // PrintMap(input, bestNodes);
    }

    // per node, get the 'previous' nodes from in the path
    // and add to our Set if they have the same overall cost
    private void AddBestPathNodes(Node node, HashSet<Node> bestNodes)
    {
        if (node.id == 'S') return;

        foreach (Node prevNode in node.prevNodes.Except(bestNodes))
        {
            if (node.id == 'E' && prevNode.cost > node.cost) continue; // special startNode case
            if (prevNode.cost < node.cost - 1001) continue; // cost too small - jump too big
            if (prevNode.prevNodes.Count == 0) continue;
            //Console.WriteLine($"{node.pos} - added {prevNode.pos}");
            bestNodes.Add(prevNode);
            AddBestPathNodes(prevNode, bestNodes);
        }
    }

    private void TryAddSiblings(Node node, List<string> input)
    {
        foreach (Vector2Int dir in _directions) 
        {
            char val = Utils.TryGetValue(input[node.pos.x + dir.x].ToCharArray(), node.pos.y + dir.y);
            if (val != '\0' && val != '#')
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
    
    // Dijkstra
    private void FindShortestPath()
    {
        List<Node> queue = new List<Node>();
        Node start = _nodes.First(node => node.id == 'S');
        start.cost = 0;
        start.dir = new Vector2Int(0, 1);
        queue.Add(start);
        
        while (queue.Count > 0)
        {
            queue.Sort();
            Node node = queue.First();
            queue.RemoveAt(0); 
            
            // check its neighbours
            List<(Node, int, Vector2Int)> possibleSteps = GetPossibleDirections(node, node.dir);
            foreach ((Node node, int cost, Vector2Int dir) nextStep in possibleSteps)
            {
                // for part 2: also add node to prevNodes if diff in cost is either 1000 or 0 and there's a direction change
                if (nextStep.cost + node.cost < nextStep.node.cost 
                    || (Math.Abs(nextStep.cost + node.cost - nextStep.node.cost) == 1000 && (nextStep.node.dir != node.dir))
                    || (Math.Abs(nextStep.cost + node.cost - nextStep.node.cost) == 0 && (nextStep.node.dir != node.dir)))
                {
                    if (nextStep.cost + node.cost < nextStep.node.cost)
                    {
                        nextStep.node.dir = nextStep.dir;
                        nextStep.node.cost = nextStep.cost + node.cost;
                        // nextStep.node.prevNode = node;
                        if (!nextStep.node.visited) queue.Add(nextStep.node);
                    }
                    nextStep.node.prevNodes.Add(node);
                }
            }

            node.visited = true;
            // Console.WriteLine(node.pos + " - " + node.cost);
        }
    }

    private List<(Node, int cost, Vector2Int dir)> GetPossibleDirections(Node node, Vector2Int dir)
    {
        List<(Node next, int cost, Vector2Int dir)> result = new();
        // ahead
        Node next = node.siblings.FirstOrDefault(n => n.pos == node.pos + dir, null);
        if (next != null) result.Add((next, 1, dir));
        // 90
        Vector2Int nextDir = _directions[(Array.IndexOf(_directions, dir) + 1) % _directions.Length];
        next = node.siblings.FirstOrDefault(n => n.pos == node.pos + nextDir, null);
        if (next != null) result.Add((next, 1001, nextDir));
        // -90
        nextDir = _directions[Utils.Mod((Array.IndexOf(_directions, dir) - 1), _directions.Length)];
        next = node.siblings.FirstOrDefault(n => n.pos == node.pos + nextDir, null);
        if (next != null) result.Add((next, 1001, nextDir));
        return result;
    }

    private void PrintMap(List<string> input, IEnumerable<Node> bestNodes)
    {
        for (int i = 0; i < input.Count(); i++)
        {
            for (int j = 0; j < input[i].Length; j++)
            {
                if (bestNodes.Any(n => n.pos == new Vector2Int(i,j))) 
                    Console.Write("O");
                else
                    Console.Write(input[i][j]);
            }
            Console.WriteLine();
        }
    }
    
    class Node : IComparable<Node>
    {
        public char id;
        public Vector2Int pos;
        public List<Node> siblings = new List<Node>();
        public List<Node> prevNodes = new List<Node>();
        public bool visited;
        public int cost = Int32.MaxValue;
        public Vector2Int dir; // direction from which we came to set lowest cost

        public Node(Vector2Int pos, char id)
        {
            this.pos = pos;
            this.id = id;
        }

        public int CompareTo(Node? other)
        {
            if (other == null) return 1;
            return cost.CompareTo(other.cost);
        }

        public int GetCost(Node target)
        {
            Vector2Int dirChange = target.dir - this.dir;
            return dirChange.Magnitude() == 0 ? 1 : 1001;
        }
    }
}