using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text.RegularExpressions;

namespace AoC2024;

public class Day16
{	
    class Node : IComparable<Node>
    {
        public char id;
        public Vector2Int pos;
        public List<Node> siblings = new List<Node>();
        public bool visited = false;
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
    }

    private static Vector2Int[] _directions = new[] { new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(0, -1), new Vector2Int(-1, 0) };
    private List<Node> _nodes = new List<Node>();
    
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
        Console.WriteLine($"Part 1: {_nodes.Count}");
        Console.WriteLine($"Part 1: {_nodes.First(node => node.id == 'E').cost}");
        // Console.WriteLine($"Part 1: {Enumerable.Range(0,_grid.GetLength(0)*_grid.GetLength(1)).Sum(i => GetGPS(i))}");
        
        // Part 2
        // Console.WriteLine($"Part 2: {Enumerable.Range(0,_grid.GetLength(0)*_grid.GetLength(1)).Sum(i => GetGPS(i))}");
        // Utils.PrintCharArray(_grid);
    }

    private void TryAddSiblings(Node node, List<string> input)
    {
        foreach (Vector2Int dir in _directions)
        {
            char val = Utils.TryGetValue(input[node.pos.x + dir.x].ToCharArray(), node.pos.y + dir.y);
            if (val != '\0' && val != '#')
            {
                Node sibling = _nodes.FirstOrDefault(n => n.pos == node.pos + dir, null);
                if (sibling != null)
                {
                    if (!sibling.siblings.Contains(node)) sibling.siblings.Add(node);
                    if (!node.siblings.Contains(sibling)) node.siblings.Add(sibling);
                }
            }
        }
    }


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
            // Vector2Int lastStep = node.lastStep;
            List<(Node, int, Vector2Int)> possibleSteps = GetPossibleDirections(node, node.dir);
            // int minHeat = directions.Min(dir => blocks[dir.x, dir.y].heatLoss);
            // foreach (Vector2Int nextDir in directions.Where(dir => blocks[dir.x, dir.y].heatLoss == minHeat))
            foreach ((Node node, int cost, Vector2Int dir) nextStep in possibleSteps)
            {
                if (!nextStep.node.visited && nextStep.cost + node.cost < nextStep.node.cost)
                {
                    nextStep.node.cost = nextStep.cost + node.cost;
                    nextStep.node.dir = nextStep.dir;
                    // nextStep.lastStep = (nextDir.Normalize() == node.lastStep.Normalize()) ? nextDir+node.lastStep : nextDir;
                    
                    // optionally store previous block
                    // nextStep.previous = node;
                    if (!nextStep.node.visited) queue.Add(nextStep.node);
                }
                //queue.Enqueue(next);
            }

            node.visited = true;
            Console.WriteLine(node.pos + " - " + node.cost);
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
}