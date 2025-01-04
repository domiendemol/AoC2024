using Spectre.Console;

namespace AoC2024;
using Node = PathFinding.Node;

public class Day21
{
    private const bool DEBUG = false;
    
    private string _doorKeys = "0123456789A";
    private List<Vector2Int> _doorKeyPositions = new List<Vector2Int>(){new Vector2Int(1,3), 
        new Vector2Int(0,2), new Vector2Int(1,2), new Vector2Int(2,2), 
        new Vector2Int(0,1), new Vector2Int(1,1), new Vector2Int(2,1),
            new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(2,0), new Vector2Int(2,3),};
    private string _robotKeys = "<v>^A";
    private List<Vector2Int> _robotKeyPositions = new List<Vector2Int>() {
        new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1),
        new Vector2Int(1, 0), new Vector2Int(2, 0)
    };
    private Vector2Int[] _doorDirections = new[] { PathFinding.Down, PathFinding.Up, PathFinding.Left, PathFinding.Right };
    private Vector2Int[] _robotDirections = new[] { PathFinding.Up, PathFinding.Down, PathFinding.Left, PathFinding.Right };

    private List<Node> _doorGraph;
    private List<Node> _robotGraph;

    private Dictionary<(char target, int depth),Sequence> _cache = new Dictionary<(char target, int depth), Sequence>();
    
    public void Run(List<string> input)
    {
        // calc shortest paths for all key combis for  the 2 keyboard designs
        // - door
        _doorGraph = BuildGraph(_doorKeys, _doorKeyPositions);
        Dictionary<(Node, Node), List<List<Node>>> doorPathLengths = new Dictionary<(Node, Node), List<List<Node>>>();
        _doorGraph.SelectMany(a => _doorGraph.Select(b => ( a, b ))).ToList().ForEach(nodeT => doorPathLengths.Add((nodeT.Item1, nodeT.Item2), PathFinding.FindShortestPaths(_doorGraph, nodeT.Item1, nodeT.Item2, _doorDirections)));
        // - robot
        _robotGraph = BuildGraph(_robotKeys, _robotKeyPositions);
        Dictionary<(Node, Node), List<List<Node>>> robotPathLengths = new Dictionary<(Node, Node), List<List<Node>>>();
        _robotGraph.SelectMany(a => _robotGraph.Select(b => ( a, b ))).ToList().ForEach(nodeT => robotPathLengths.Add((nodeT.Item1, nodeT.Item2), PathFinding.FindShortestPaths(_robotGraph, nodeT.Item1, nodeT.Item2, _robotDirections)));
       
        // TODO later: take into account every robot returns to A (to optimize)
        
        List<string> fullPath = GetFullSplitRobotSequence("456A", 3, robotPathLengths, doorPathLengths);
        Console.WriteLine($"Part 1, length:   {fullPath.Sum(part => part.Length)}");
        Console.WriteLine($"Part 1, expect:   64");
        Console.WriteLine($"Part 1, shortest: {string.Join(", ", fullPath)}");
        
        long complexity = 0;
        foreach (string line in input)
        {
            List<string> fullSeq = GetFullSplitRobotSequence(line, 3, robotPathLengths, doorPathLengths);
            Console.WriteLine(fullSeq.Sum(part => part.Length) + " * " +int.Parse(line.Substring(0, 3)));
            complexity += fullSeq.Sum(part => part.Length) * int.Parse(line.Substring(0, 3));
        }
        Console.WriteLine($"Part 1: {complexity}");

        for (int i = 4; i < 6; i++)
        {
            fullPath = GetFullSplitRobotSequence("456A", i, robotPathLengths, doorPathLengths);
            Console.WriteLine(i);
        }
        // fullPath = GetFullSplitRobotSequence("456A", 3, robotPathLengths, doorPathLengths);
        // Console.WriteLine($"Part 2, shortest: {string.Join(", ", fullPath)}");
        fullPath = GetFullSplitRobotSequence("456A", 10, robotPathLengths, doorPathLengths);
        Console.WriteLine($"Part 2, shortest: {string.Join(", ", fullPath)}");

    }

    private List<string> GetFullSplitRobotSequence(string code, int nrRobots, Dictionary<(Node, Node), List<List<Node>>> robotPathLengths, Dictionary<(Node, Node), List<List<Node>>> doorPathLengths)
    {
        List<string> result = new List<string>();
        
        // for each CODE key
        for (int i = 0; i < code.Length; i++) 
        {
            List<Sequence> startSeq = new List<Sequence>(){new Sequence(26)};
            Node doorStart = _doorGraph.Find(n => n.id == (i== 0 ? 'A' : code[i-1]))!; 
            Node robotStart = _robotGraph.Find(n => n.id == 'A')!; // A
            Node[] robotPositions = new Node[26]; // fill with robotStart node (except first one)
            Enumerable.Range(0, 26).ToList().ForEach(i => robotPositions[i] = i == 0 ? doorStart : robotStart);
            startSeq[0].robotPositions = robotPositions;
            
            var robotPaths = GetRobotSequence(code[i], doorPathLengths, robotPathLengths, startSeq, nrRobots, 0).OrderBy(seq => seq.path.Length).ToList();
            if (DEBUG) Console.WriteLine($"{code[i]}: {robotPaths.Count}");
            result.Add(robotPaths.First().path);
        }

        return result;
    }
    
    // do it for every path/sequence we found so far
    private List<Sequence> GetRobotSequence(char targetOutputKey, Dictionary<(Node, Node), List<List<Node>>> doorPathLengths, Dictionary<(Node, Node), List<List<Node>>> robotPathLengths,
                                            List<Sequence> prevSeqList, int nrRobots, int robot)
    {
        return prevSeqList.SelectMany(prevSequence => GetRobotSequence(targetOutputKey, doorPathLengths, robotPathLengths, prevSequence, nrRobots, robot)).ToList();
    }
    
    private List<Sequence> GetRobotSequence(char targetOutputKey, Dictionary<(Node, Node), List<List<Node>>> doorPathLengths, Dictionary<(Node, Node), List<List<Node>>> robotPathLengths,
                                        Sequence previousSequence, int nrRobots, int robot)
    {
        if (robot == nrRobots - 1) return GetFinalRobotSequence(targetOutputKey, doorPathLengths, robotPathLengths, previousSequence, nrRobots, robot);
        if (robot > 0 && 
            previousSequence.robotPositions[robot].id == 'A' 
            && _cache.ContainsKey((targetOutputKey, nrRobots-robot))) return new List<Sequence>() { _cache[(targetOutputKey, nrRobots-robot)] };
        
        List<Sequence> finalInputSeqs = new List<Sequence>(){};

        // find targetKeyboardNode
        var targetKeyboardGraph = robot == 0 ? _doorGraph : _robotGraph;
        var pathLengths = robot == 0 ? doorPathLengths : robotPathLengths;
        
        Node endNode = targetKeyboardGraph.Find(n => n.id == targetOutputKey)!;
        List<List<Node>>? paths = pathLengths[(previousSequence.robotPositions[robot], endNode)]; // paths to simulate (on output keyboard)
        
        string pathDebug = paths == null ? "/" : string.Join(',', paths.Select(list => string.Join("", list)));
        if (DEBUG) Console.WriteLine($"GetRobotSequence key:{targetOutputKey} robot:{robot}  path:{previousSequence.robotPositions[robot]} -> {endNode} ({pathDebug} options)");
        
        // handle multiple (output) paths
        foreach (List<Node> path in paths ?? Enumerable.Empty<List<Node>>())
        {
            Sequence newSequence = new Sequence(previousSequence);
            List<Sequence> newStepSeqs = new List<Sequence>(){newSequence}; // for this step in this path
            foreach (Node step in path)
            {
                char newTargetKey = GetNextKey(step.pos, newSequence.robotPositions[robot].pos);
                newSequence.robotPositions[robot] = step;

                newStepSeqs = newStepSeqs.Select(seq => new Sequence(seq)).ToList();
                newStepSeqs.ForEach(seq => seq.robotPositions[robot] = step);

                List<Sequence> nextRobotPaths = GetRobotSequence(newTargetKey, doorPathLengths, robotPathLengths, newStepSeqs, nrRobots, robot + 1);
                newStepSeqs = nextRobotPaths;
            }
            finalInputSeqs.AddRange(newStepSeqs);
        }
        
        if (finalInputSeqs.Count == 0) finalInputSeqs.Add(previousSequence);
        
        // finally, press A to confirm
        finalInputSeqs = GetRobotSequence('A', doorPathLengths, robotPathLengths, finalInputSeqs, nrRobots, robot+1);
        
        if (DEBUG) finalInputSeqs.ForEach(seq => Console.WriteLine($"GetRobotSequence key:{targetOutputKey} robot:{robot} {string.Join(',', seq.path)}"));
        if (robot > 0 && previousSequence.robotPositions[robot].id == 'A' && !_cache.ContainsKey((targetOutputKey, nrRobots-robot))) _cache[(targetOutputKey, nrRobots-robot)] = finalInputSeqs.OrderBy(s => s.path.Length).First();
        return finalInputSeqs;
    }

    private List<Sequence> GetFinalRobotSequence(char targetOutputKey, Dictionary<(Node, Node), List<List<Node>>> doorPathLengths, Dictionary<(Node, Node), List<List<Node>>> robotPathLengths,
                                                    Sequence previousSequence, int nrRobots, int robot)
    {
        // find targetKeyboardNode
        var targetKeyboardGraph = nrRobots == 1 ? _doorGraph : _robotGraph;
        var pathLengths = nrRobots == 1 ? doorPathLengths : robotPathLengths;
        
        Node endNode = targetKeyboardGraph.Find(n => n.id == targetOutputKey)!;
        List<List<Node>>? paths = pathLengths[(previousSequence.robotPositions[robot], endNode)]; // paths to simulate (on previous keyboard)
        
        string pathDebug = paths == null ? "/" : string.Join(',', paths.Select(list => string.Join("", list)));
        if (DEBUG) Console.WriteLine($"GetFinalRobotSequence key:{targetOutputKey} robot:{robot}  path:{previousSequence.robotPositions[robot]} -> {endNode} ({pathDebug} options)");
        
        // handle multiple (output) paths
        List<Sequence> thisRobotInputs = new List<Sequence>();
        foreach (List<Node> path in paths ?? Enumerable.Empty<List<Node>>())
        {
            Sequence newSequence = new Sequence(previousSequence);
            foreach (Node step in path)
            {
                char newTargetKey = GetNextKey(step.pos, newSequence.robotPositions[robot].pos);
                newSequence = new Sequence(newSequence);
                newSequence.AddPath(""+newTargetKey, robot, step);
            }
            thisRobotInputs.Add(newSequence);
        }
        
        if (thisRobotInputs.Count == 0) thisRobotInputs.Add(previousSequence);

        // finally press A (on input) to confirm this key
        for (int i = 0; i < thisRobotInputs.Count; i++) {
            thisRobotInputs[i].path += 'A';
        }
        
        if (DEBUG) thisRobotInputs.ForEach(seq => Console.WriteLine($"GetFinalRobotSequence key:{targetOutputKey} robot:{robot} {string.Join(',', seq.path)}"));
        return thisRobotInputs;
    }

    private char GetNextKey(Vector2Int newPos, Vector2Int oldPos)
    {
        Vector2Int diff = newPos - oldPos;
        char newTargetKey = 'A';
        if (diff == PathFinding.Up) newTargetKey = '^'; 
        if (diff == PathFinding.Down) newTargetKey = 'v'; 
        if (diff == PathFinding.Left) newTargetKey = '<'; 
        if (diff == PathFinding.Right) newTargetKey = '>';
        return newTargetKey;
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

    public class Sequence
    {
        public string path;
        public int lastRobot;
        public Node[] robotPositions;

        public Sequence(int nrRobots)
        {
            this.robotPositions = new Node[26];
            // Node doorStart = doorGraph.Find(n => n.id == 'A')!; // A
            // Node robotStart = robotGraph.Find(n => n.id == 'A')!; // A
        }
        public Sequence(Sequence oldSequence)
        {
            path = oldSequence.path;
            lastRobot = oldSequence.lastRobot;
            robotPositions = (Node[]) oldSequence.robotPositions.Clone();
        }

        public void AddPath(string newPath, int robot, Node robotPosition)
        {
            path += newPath;
            lastRobot = robot;
            robotPositions[robot] = robotPosition;
        }
    }
}