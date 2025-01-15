using System.Collections.Concurrent;
using System.Diagnostics;
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
    private Dictionary<(Node, Node), List<List<Node>>> _doorPathLengths;
    private Dictionary<(Node, Node), List<List<Node>>> _robotPathLengths;

    private Dictionary<(string, int depth), string> _cache = new Dictionary<(string, int depth), string>();
    
    public void Run(List<string> input)
    {
        // calc shortest paths for all key combis for  the 2 keyboard designs
        // - door
        _doorGraph = BuildGraph(_doorKeys, _doorKeyPositions);
        _doorPathLengths = new Dictionary<(Node, Node), List<List<Node>>>();
        _doorGraph.SelectMany(a => _doorGraph.Select(b => ( a, b ))).ToList().ForEach(nodeT => _doorPathLengths.Add((nodeT.Item1, nodeT.Item2), PathFinding.FindShortestPaths(_doorGraph, nodeT.Item1, nodeT.Item2, _doorDirections)));
        // - robot
        _robotGraph = BuildGraph(_robotKeys, _robotKeyPositions);
        _robotPathLengths = new Dictionary<(Node, Node), List<List<Node>>>();
        _robotGraph.SelectMany(a => _robotGraph.Select(b => ( a, b ))).ToList().ForEach(nodeT => _robotPathLengths.Add((nodeT.Item1, nodeT.Item2), PathFinding.FindShortestPaths(_robotGraph, nodeT.Item1, nodeT.Item2, _robotDirections)));

        
        List<string> fullPath;
        fullPath = GetFullSplitRobotSequence("029A", 1);
        Console.WriteLine($"1, shortest: {string.Join(", ", fullPath).Length} - {string.Join(", ", fullPath)}");
        fullPath = GetFullSplitRobotSequence("029A", 2);
        Console.WriteLine($"2, shortest: {string.Join(", ", fullPath).Length} - {string.Join(", ", fullPath)}");
        
        fullPath = GetFullSplitRobotSequence("029A", 3);
        Console.WriteLine($"3, shortest: {string.Join(", ", fullPath).Length} - {string.Join(", ", fullPath)}");
        
        fullPath = GetFullSplitRobotSequence("0A", 4);
        Console.WriteLine($"4, shortest: {string.Join(", ", fullPath).Length} - {string.Join(", ", fullPath)}");
        
        fullPath = GetFullSplitRobotSequence("0A", 5);
        Console.WriteLine($"5, shortest: {string.Join(", ", fullPath).Length} - {string.Join(", ", fullPath)}");
        
        fullPath = GetFullSplitRobotSequence("0A", 6);
        Console.WriteLine($"6, shortest: {string.Join(", ", fullPath).Length} - {string.Join(", ", fullPath)}");
/*
        //for (int i = 4; i < 26; i++)
        //    GetFullSplitRobotSequence("0", i);

        // fullPath = GetFullSplitRobotSequence("0", 4);
        Console.WriteLine($"Part 1, length:   {fullPath.Sum(s => s.Length)}");
        Console.WriteLine($"Part 1, expect:   64");
        Console.WriteLine($"Part 1, shortest: {string.Join(", ", fullPath)}");

        _cache.Keys.ToList().ForEach(nodeT => Console.WriteLine($"{(nodeT)} -> {_cache[(nodeT)]}"));
*/
        // part 1
        long complexity = 0;
        foreach (string line in input)
        {
            List<string> fullSeq = GetFullSplitRobotSequence(line, 3);
            Console.WriteLine(fullSeq.Sum(s => s.Length) + " * " +int.Parse(line.Substring(0, 3)));
            complexity += fullSeq.Sum(s => s.Length) * int.Parse(line.Substring(0, 3));
        }
        Console.WriteLine($"Part 1: {complexity}");

        // part 2
        complexity = 0;
         for (int i = 4; i <= 26; i++)
         {
             foreach (string line in input)
             {
                 List<string> fullSeq = GetFullSplitRobotSequence(line, i);
                 // if (i == 4) Console.WriteLine(fullSeq.Sum(s => s.Length) + " * " + long.Parse(line.Substring(0, 3)));
                 if (i == 26) complexity += fullSeq.Sum(s => s.Length) * long.Parse(line.Substring(0, 3));
             }
         }
         Console.WriteLine($"Part 2: {complexity}");
         
    }

    private List<string> GetFullSplitRobotSequence(string code, int nrRobots)
    {
        List<string> result = new List<string>();
        var robotPath = GetRobotSequence(code, nrRobots, 0);
        result.Add(robotPath);
        return result;
    }
    
    
    private string GetRobotSequence(string pattern, int nrRobots, int robot)
    {
        if (robot == nrRobots - 1) return GetFinalRobotSequence(pattern, nrRobots, robot).MinBy(str => str.Length);

        if (robot > 0 && _cache.ContainsKey((pattern, nrRobots-robot)))
        {
            string cached = _cache[(pattern, nrRobots-robot)];
            // Console.WriteLine($"CACHE get {pattern}, {nrRobots-robot}, {cached}");
            return cached;
        }

        List<string> finalInputSeqs = new List<string>(){""};
        Node prevNode = (robot == 0 ? _doorGraph : _robotGraph).Find(n => n.id == 'A');
        foreach (char c in pattern) {
            finalInputSeqs = finalInputSeqs.SelectMany(prevSeq => GetRobotSequence(c, prevSeq, prevNode, nrRobots, robot)).ToList();
            prevNode = (robot == 0 ? _doorGraph : _robotGraph).Find(n => n.id == c);
        }
        string result = finalInputSeqs.MinBy(str => str.Length);
        
        if (robot > 0 && !_cache.ContainsKey((pattern, nrRobots-robot))) { 
            TryAddToCache(pattern, '-', robot, nrRobots, result);
        }
        
        return result;
    }
    
    private List<string> GetRobotSequence(char targetOutputKey, string prevLength, Node prevNode, int nrRobots, int robot)
    {
        if (robot == nrRobots - 1) return GetFinalRobotSequence(targetOutputKey, prevNode, nrRobots).Select(l => prevLength + l).ToList();

        List<string> finalInputSeqs = new List<string>(){};

        // find targetKeyboardNode
        var targetKeyboardGraph = robot == 0 ? _doorGraph : _robotGraph;
        var pathLengths = robot == 0 ? _doorPathLengths : _robotPathLengths;
        
        Node endNode = targetKeyboardGraph.Find(n => n.id == targetOutputKey)!;
        List<List<Node>>? paths = pathLengths[(prevNode, endNode)]; // paths to simulate (on output keyboard)
        
        string pathDebug = paths == null ? "/" : string.Join(',', paths.Select(list => string.Join("", list)));
        if (DEBUG) Console.WriteLine($"GetRobotSequence key:{targetOutputKey} robot:{robot}  -> {endNode} ({pathDebug} options)");
        
        // handle multiple (output) paths
        foreach (List<Node> path in paths ?? Enumerable.Empty<List<Node>>())
        {
            string pathString = string.Join("", path.Select(n => n.id));
            string nextRobotPathString = "";
            Node prevPos = prevNode;
            foreach (Node step in path)
            {
                nextRobotPathString += GetNextKey(step.pos, prevPos.pos);
                prevPos = step;
            }
            finalInputSeqs.Add(GetRobotSequence(nextRobotPathString+"A", nrRobots, robot+1));
            // foreach (Node step in path)
            // {
            //     char newTargetKey = GetNextKey(step.pos, newSequence.robotPositions[robot].pos);
            //     newSequence.robotPositions[robot] = step;
            //     newStepSeqs = newStepSeqs.Select(seq => new Sequence(seq)).ToList();
            //     newStepSeqs.ForEach(seq => seq.robotPositions[robot] = step);
            //
            //     List<Sequence> nextRobotPaths = newStepSeqs.SelectMany(prevSeq => GetRobotSequence(newTargetKey, prevSeq, nrRobots, robot + 1)).ToList();
            //     newStepSeqs = nextRobotPaths;          
            // }
            // finalInputSeqs.AddRange(newStepSeqs);
        }
        if (finalInputSeqs.Count == 0)
        {
            prevNode = _robotGraph.Find(n => n.id == 'A');
            finalInputSeqs.AddRange(GetRobotSequence('A', "", prevNode, nrRobots, robot+1));
        }

        return finalInputSeqs.Select(l => prevLength + l).ToList();
    }

    private void TryAddToCache(string fromChar, char targetOutputKey, int robot, int nrRobots, string newLength)
    {
        if (DEBUG) Console.WriteLine($"CACHE ADD {fromChar}, {nrRobots-robot}, {newLength}");
        _cache[(fromChar, nrRobots - robot)] = newLength;
    }

    private List<string> GetFinalRobotSequence(string pattern, int nrRobots, int robot)
    {
        string result = "";
        Node prevNode = (robot == 0 ? _doorGraph : _robotGraph).Find(n => n.id == 'A')!;
        foreach (char c in pattern)
        {
            result += GetFinalRobotSequence(c, prevNode, nrRobots).MinBy(str => str.Length);
            prevNode = (robot == 0 ? _doorGraph : _robotGraph).Find(n => n.id == c)!;
        }
        return new List<string>(){result};
    }
    
    private List<string> GetFinalRobotSequence(char targetOutKey, Node prevNode, int nrRobots)
    {
        // find targetKeyboardNode
        var targetKeyboardGraph = nrRobots == 1 ? _doorGraph : _robotGraph;
        var pathLengths = nrRobots == 1 ? _doorPathLengths : _robotPathLengths;
        
        Node endNode = targetKeyboardGraph.Find(n => n.id == targetOutKey)!;
        List<List<Node>>? paths = pathLengths[(prevNode, endNode)]; // paths to simulate (on previous keyboard)
        
        string pathDebug = paths == null ? "/" : string.Join(',', paths.Select(list => string.Join("", list)));
        
        // handle multiple (output) paths
        List<string> thisRobotInputs = new List<string>();
        foreach (List<Node> path in paths ?? Enumerable.Empty<List<Node>>())
        {
            string t ="";
            Node prevPos = prevNode;
            foreach (Node step in path)
            {
                char newTargetKey = GetNextKey(step.pos, prevPos.pos);
                prevPos = step;
                t += newTargetKey;
            }
            thisRobotInputs.Add(t);
        }
        
        if (thisRobotInputs.Count == 0) thisRobotInputs.Add("");

        // finally press A (on input) to confirm this key
        for (int i = 0; i < thisRobotInputs.Count; i++) {
            thisRobotInputs[i]+='A'; // += 'A';
        }

        return thisRobotInputs;
    }

    private string Shortest(List<string> sequences) => sequences.MinBy(str => str.Length);
    
    private char GetNextKey(Vector2Int newPos, Vector2Int oldPos)
    {
        Vector2Int diff = newPos - oldPos;
        char newTargetKey = 'A';
        if (diff == PathFinding.Up) newTargetKey = '^'; 
        else if (diff == PathFinding.Down) newTargetKey = 'v'; 
        else if (diff == PathFinding.Left) newTargetKey = '<'; 
        else if (diff == PathFinding.Right) newTargetKey = '>';
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
}