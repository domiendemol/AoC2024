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
        Dictionary<(Node, Node), List<List<Node>>> doorPathLengths = new Dictionary<(Node, Node), List<List<Node>>>();
        doorGraph.SelectMany(a => doorGraph.Select(b => ( a, b ))).ToList().ForEach(nodeT => doorPathLengths.Add((nodeT.Item1, nodeT.Item2), PathFinding.FindShortestPaths(doorGraph, nodeT.Item1, nodeT.Item2, doorDirections)));
        // robot
        List<Node> robotGraph = BuildGraph(_robotKeys, _robotKeyPositions);
        Dictionary<(Node, Node), List<List<Node>>> robotPathLengths = new Dictionary<(Node, Node), List<List<Node>>>();
        robotGraph.SelectMany(a => robotGraph.Select(b => ( a, b ))).ToList().ForEach(nodeT => robotPathLengths.Add((nodeT.Item1, nodeT.Item2), PathFinding.FindShortestPaths(robotGraph, nodeT.Item1, nodeT.Item2, robotDirections)));

        // var testPaths = doorPathLengths[(doorGraph.Find(n => n.id == 'A'), doorGraph.Find(n => n.id == '7'))];
        // foreach (List<Node> testPath in testPaths) {
            // Console.WriteLine(string.Join(',', testPath));
        // }
        
        List<string> fullSeq = GetFullRobotSequence("029A", 2, robotPathLengths, doorPathLengths, doorGraph, robotGraph);
        fullSeq.ForEach(str => Console.WriteLine($"Part 1: {string.Join(',', str)}"));

        return;
        foreach (string line in input)
        {
            fullSeq = GetFullRobotSequence(line, 3, robotPathLengths, doorPathLengths, doorGraph, robotGraph);
            // TODO get complexity
        }
    }

    private List<string> GetFullRobotSequence(string code, int nrRobots, Dictionary<(Node, Node), List<List<Node>>> robotPathLengths, Dictionary<(Node, Node), List<List<Node>>> doorPathLengths, List<Node> doorGraph, List<Node> robotGraph)
    {
        List<string> totalSequences = new List<string>(){""};
        Node doorStart = doorGraph.Find(n => n.id == 'A')!; // A
        Node robotStart = robotGraph.Find(n => n.id == 'A')!; // A
        Node[] robotPositions = new Node[nrRobots]; // fill with robotStart node (except first one)
        Enumerable.Range(0, nrRobots).ToList().ForEach(i => robotPositions[i] = i == 0 ? doorStart : robotStart);
        
        // for each CODE key
        for (int i = 0; i < code.Length; i++) {
            var robotPaths = GetRobotSequence(code[i], doorPathLengths, robotPathLengths, doorGraph, robotGraph, ref robotPositions, nrRobots, 0);
            // add every robot path to every 
            int totalSeqCount = totalSequences.Count;
            for (int j = 0; j < totalSeqCount; j++) {
                Console.WriteLine($"GetFullRobotSequence: key: {code[i]}, adding {totalSeqCount} possible sequences");
                for (int k = 0; k < robotPaths.Count; k++) {
                    if (k == 0) totalSequences[j] += robotPaths[k]; // add to existing one
                    else totalSequences.Add(totalSequences[j] + robotPaths[k]); // add new path
                }
            }
            // totalPath += robotPath;
        }

        return totalSequences;
    }

    private List<string> GetRobotSequence(char targetKey, Dictionary<(Node, Node), List<List<Node>>> doorPathLengths, Dictionary<(Node, Node), List<List<Node>>> robotPathLengths,
                                        List<Node> doorGraph, List<Node> robotGraph, ref Node[] robotPositions, int nrRobots, int robot)
    {
        string finalInput = "";
        List<string> finalInputs = new List<string>(){""};

        // find targetKeyboardNode
        var targetKeyboardGraph = robot == 0 ? doorGraph : robotGraph;
        var pathLengths = robot == 0 ? doorPathLengths : robotPathLengths;
        
        Node endNode = targetKeyboardGraph.Find(n => n.id == targetKey)!;
        List<List<Node>>? paths = pathLengths[(robotPositions[robot], endNode)]; // paths to simulate (on previous keyboard)
        
        // PrintPath(path);        
        string pathDebug = paths == null ? "/" : string.Join(',', paths.Select(list => string.Join("", list)));
        Console.WriteLine($"GetRobotSequence key:{targetKey} robot:{robot}  path:{robotPositions[robot]} -> {endNode} ({pathDebug} options)");
        
        // handle multiple paths
        List<string> thisRobotInputs = new List<string>();
        foreach (List<Node> path in paths ?? Enumerable.Empty<List<Node>>())
        {
            string thisRobotInputPPath = "";
            foreach (Node step in path)
            {
                Vector2Int diff = step.pos - robotPositions[robot].pos;
                char newTargetKey = 'A';
                if (diff == PathFinding.Up) newTargetKey = '^'; 
                if (diff == PathFinding.Down) newTargetKey = 'v'; 
                if (diff == PathFinding.Left) newTargetKey = '<'; 
                if (diff == PathFinding.Right) newTargetKey = '>';
                thisRobotInputPPath += newTargetKey;    
            
                if (robot < nrRobots-1)
                {
                    List<string> nextRobotPaths = GetRobotSequence(newTargetKey, doorPathLengths, robotPathLengths, doorGraph, robotGraph, ref robotPositions, nrRobots, robot+1);
                    // TODO add this for every path so far
                    foreach (string nextRobotPath in nextRobotPaths)
                    {
                        int finalInputCount = finalInputs.Count;
                        for (int i = 0; i < finalInputCount; i++) {
                            if (i == 0) finalInputs[i] += nextRobotPath;
                            else finalInputs.Add(finalInputs[i] + nextRobotPath);
                        }
                    }
                }
            
                robotPositions[robot] = step; // TODO check this?? FIX IT
            }            
            thisRobotInputs.Add(thisRobotInputPPath);
        }
        if (thisRobotInputs.Count == 0) thisRobotInputs.Add("");
        
        // finally press A to confirm
        for (int i = 0; i < thisRobotInputs.Count; i++) {
            thisRobotInputs[i] += 'A';
        }
        if (robot < nrRobots-1)
        {
            List<string> nextRobotPaths = GetRobotSequence('A', doorPathLengths, robotPathLengths, doorGraph, robotGraph, ref robotPositions, nrRobots, robot+1);
            // handle multiple paths
            foreach (string nextRobotPath in nextRobotPaths)
            {
                int finalInputCount = finalInputs.Count;
                for (int i = 0; i < finalInputCount; i++) {
                    if (i == 0) finalInputs[i] += nextRobotPath;
                    else finalInputs.Add(finalInputs[i] + nextRobotPath);
                }
            }
        }
        
        (robot == nrRobots - 1 ? thisRobotInputs : finalInputs).ForEach(str => Console.WriteLine($"GetRobotSequence key:{targetKey} robot:{robot} {string.Join(',', str)}"));
        return robot == nrRobots - 1 ? thisRobotInputs : finalInputs;
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
    }
}