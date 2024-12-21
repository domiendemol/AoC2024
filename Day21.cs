namespace AoC2024;
using Node = PathFinding.Node;

public class Day21
{
    private string _doorKeys = "0123456789A";
    private List<Vector2Int> _doorKeyPositions = new List<Vector2Int>(){new Vector2Int(1,3), 
        new Vector2Int(0,2), new Vector2Int(1,2), new Vector2Int(2,2), 
        new Vector2Int(0,1), new Vector2Int(1,1), new Vector2Int(2,1),
            new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(2,0), new Vector2Int(2,3),};
    private string _robotKeys = "<v>^A";
    private List<Vector2Int> _robotKeyPositions = new List<Vector2Int>() {
        new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(1, 2),
        new Vector2Int(1, 0), new Vector2Int(2, 0)
    };
    private Vector2Int[] doorDirections = new[] { PathFinding.Down, PathFinding.Up, PathFinding.Left, PathFinding.Right };
    private Vector2Int[] robotDirections = new[] { PathFinding.Left, PathFinding.Up, PathFinding.Down, PathFinding.Right };
    
    public void Run(List<string> input)
    {
        // calc shortest paths for all key combis for  the 2 keyboard designs
        // door
        List<Node> doorGraph = BuildGraph(_doorKeys, _doorKeyPositions);
        Dictionary<(Node, Node), List<Node>> doorPathLengths = new Dictionary<(Node, Node), List<Node>>();
        doorGraph.SelectMany(a => doorGraph.Select(b => ( a, b ))).ToList().ForEach(nodeT => doorPathLengths.Add((nodeT.Item1, nodeT.Item2), PathFinding.FindShortestPaths(doorGraph, nodeT.Item1, nodeT.Item2, doorDirections)));
        // robot
        List<Node> robotGraph = BuildGraph(_robotKeys, _robotKeyPositions);
        Dictionary<(Node, Node), List<Node>> robotPathLengths = new Dictionary<(Node, Node), List<Node>>();
        robotGraph.SelectMany(a => robotGraph.Select(b => ( a, b ))).ToList().ForEach(nodeT => robotPathLengths.Add((nodeT.Item1, nodeT.Item2), PathFinding.FindShortestPaths(robotGraph, nodeT.Item1, nodeT.Item2, robotDirections)));
    
        string fullSeq = GetFullRobotSequence("029A", 3, robotPathLengths, doorPathLengths, doorGraph, robotGraph);
        Console.WriteLine($"Part 1: {fullSeq}");

        return;
        foreach (string line in input)
        {
            fullSeq = GetFullRobotSequence(line, 3, robotPathLengths, doorPathLengths, doorGraph, robotGraph);
            // TODO get complexity
        }
    }

    private string GetFullRobotSequence(string code, int nrRobots, Dictionary<(Node, Node), List<Node>> robotPathLengths, Dictionary<(Node, Node), List<Node>> doorPathLengths, List<Node> doorGraph, List<Node> robotGraph)
    {
        string totalPath = "";
        Node doorStart = doorGraph.Find(n => n.id == 'A')!; // A
        Node robotStart = robotGraph.Find(n => n.id == 'A')!; // A
        Node[] robotPositions = new Node[nrRobots]; // fill with robotStart node (except first one)
        Enumerable.Range(0, nrRobots).ToList().ForEach(i => robotPositions[i] = i == 0 ? doorStart : robotStart);
        
        // for each CODE key
        for (int i = 0; i < code.Length; i++) {
            var robotPath = GetRobotSequence(code[i], doorPathLengths, robotPathLengths, doorGraph, robotGraph, ref robotPositions, nrRobots, 0);
            totalPath += robotPath;
        }

        return totalPath;
    }

    private string GetRobotSequence(char targetKey, Dictionary<(Node, Node), List<Node>> doorPathLengths, Dictionary<(Node, Node), List<Node>> robotPathLengths,
                                        List<Node> doorGraph, List<Node> robotGraph, ref Node[] robotPositions, int nrRobots, int robot)
    {
        string finalInput = "";

        // find targetKeyboardNode
        var targetKeyboardGraph = robot == 0 ? doorGraph : robotGraph;
        var pathLengths = robot == 0 ? doorPathLengths : robotPathLengths;
        
        Node endNode = targetKeyboardGraph.Find(n => n.id == targetKey)!;
        Console.WriteLine($"GetRobotSequence key:{targetKey} robot:{robot}  path:{robotPositions[robot]} -> {endNode}");
        
        List<Node> path = pathLengths[(robotPositions[robot], endNode)]; // path to simulate (on previous keyboard)
        // PrintPath(path);        

        string thisRobotInput = "";
        foreach (Node step in path)
        {
            Vector2Int diff = step.pos - robotPositions[robot].pos;
            char newTargetKey = 'A';
            if (diff == PathFinding.Up) newTargetKey = '^'; 
            if (diff == PathFinding.Down) newTargetKey = 'v'; 
            if (diff == PathFinding.Left) newTargetKey = '<'; 
            if (diff == PathFinding.Right) newTargetKey = '>';
            thisRobotInput += newTargetKey;
            
            if (robot < nrRobots-1)
            {
                string nextRobotPath = GetRobotSequence(newTargetKey, doorPathLengths, robotPathLengths, doorGraph, robotGraph, ref robotPositions, nrRobots, robot+1);
                finalInput = finalInput + nextRobotPath;
            }
            
            robotPositions[robot] = step;
        }
        
        // finally press A to confirm
        thisRobotInput += 'A';
        if (robot < nrRobots-1)
        {
            string nextRobotPath = GetRobotSequence('A', doorPathLengths, robotPathLengths, doorGraph, robotGraph, ref robotPositions, nrRobots, robot+1);
            finalInput = finalInput + nextRobotPath;
        }
        
        return robot == nrRobots - 1 ? thisRobotInput : finalInput;
    }
    
    List<Node> BuildGraph(string keys, List<Vector2Int> keyPositions)
    {
        List<Node> nodes = new List<Node>();
        // build nodes
        keys.ToList().ForEach(k => nodes.Add(new Node(keyPositions[keys.IndexOf(k)], k)));
        // build siblings/edges
        foreach (Node node in nodes) {
            foreach (Vector2Int direction in PathFinding.directions) {
                Node sibling = nodes.FirstOrDefault(n => n.pos == node.pos + direction, null);
                if (sibling != null) {
                    if (!node.siblings.Contains(sibling)) node.siblings.Add(sibling);
                    if (!sibling.siblings.Contains(node)) sibling.siblings.Add(node);
                }
            }
        }
        return nodes;
    }

    private void PrintPath(List<Node> nodes)
    {
        nodes.ForEach(n => Console.Write(n.id + " "));
        Console.WriteLine();
        // nodes.ForEach(n => Console.Write(_doorKeys[n.id] + " "));
        // Console.WriteLine();
    }
}