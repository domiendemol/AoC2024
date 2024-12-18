using System.Text.RegularExpressions;

namespace AoC2024;

public class Day18
{
    private static Vector2Int[] _directions = [new(-1, 0), new(0, 1), new(1, 0), new(0,-1)];

    public void Run(List<string> input)
    {
        List<Vector2Int> memories = input.Select(x => new Vector2Int(int.Parse(x.Split(',')[0]), int.Parse(x.Split(',')[1]))).ToList();
        
        // limit grid and memories to read
        int gridSize = memories[0].x == 24 ? 70 : 6;
        List<Vector2Int> firstMemories = memories.Take(memories[0].x == 24 ? 1024 : 12).ToList();     
        
        // build graph/nodes
        List<Node> graph = new List<Node>();
        for (int x = 0; x <= gridSize; x++) {
            for (int y = 0; y <= gridSize; y++) {
                if (!firstMemories.Contains(new Vector2Int(x, y))) graph.Add(new Node(new Vector2Int(x, y)));
            }
        }
        
        // Part 1 
        Node end = graph.First(n => n.pos == new Vector2Int(gridSize, gridSize));
        int shortestPath = GetPathLength(graph, end, -1);
        Console.WriteLine($"Part 1: {shortestPath}");
        
        // Part 2
        int runs = 0;
        // List<Node> reducingGraph = new List<Vector2Int>(graph);
        for (int i = firstMemories.Count; i < memories.Count; i++)
        {
            Node node = graph.FirstOrDefault(n => n.pos == memories[i], null);
            if (node != null)
            {
                graph.Remove(node); // remove from graph
                // optimizations, only run path finding after removing relevant nodes (not on path and with low enough cost)
                if (!node.onPath) continue;
                if (node.cost + (end.pos - node.pos).Magnitude() > shortestPath) continue;
                
                // graph.ForEach(n => n.Reset());
                end.Reset(runs);
                shortestPath = GetPathLength(graph, end, runs++);
                 Console.WriteLine($"Part 2, removed : {memories[i]} ==> {shortestPath}");
                if (shortestPath == Int32.MaxValue)
                {
                    Console.WriteLine($"Part 2: {memories[i]}");
                    Console.WriteLine($"PF runs: {runs}");
                    return;
                }
            }
        }
    }

    private int GetPathLength(List<Node> graph, Node end, int run)
    {
        Queue<Node> queue = new Queue<Node>();
        Node start = graph.First(n => n.pos == new Vector2Int(0, 0));
        start.cost = 0;
        queue.Enqueue(start);

        while (queue.Count > 0)
        {   
            Node node = queue.Dequeue();
            if (node.cost + (end.pos - node.pos).Magnitude() >= end.cost) continue;

            for (int i = 0; i < _directions.Length; i++)
            {
                Node next = graph.FirstOrDefault(n => n.pos == node.pos + _directions[i], null);
                if (next != null && next.run != run) next.Reset(run);
                if (next != null && node.cost + 1 < next.cost)
                {
                    next.cost = node.cost + 1;
                    next.prevNode = node;
                    if (next != end) queue.Enqueue(next);
                }
            }
        }
        
        // mark path, going from end to start
        Node pathNode = end.prevNode;
        while (pathNode != start)
        {
            pathNode.onPath = true;
            pathNode = pathNode.prevNode;
        }
        
        return end.cost;
    }

    class Node
    {
        public Vector2Int pos;
        public Node prevNode;
        public int cost = Int32.MaxValue;
        public bool onPath = false;
        public int run = 0;

        public Node(Vector2Int pos)
        {
            this.pos = pos;
        }

        public void Reset(int run)
        {
            cost = Int32.MaxValue;
            onPath = false;
            this.run = run;
        }
    }
}