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

	private Dictionary<(string, int depth), long> _cache = new Dictionary<(string, int depth), long>();
    
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

		// part 1
		long complexity = 0;
		foreach (string line in input)
		{
			long fullSeq = GetFullRobotSequence(line, 3);
			if (DEBUG) Console.WriteLine(fullSeq + " * " +int.Parse(line.Substring(0, 3)));
			complexity += fullSeq * int.Parse(line.Substring(0, 3));
		}
		Console.WriteLine($"Part 1: {complexity}");

		if (DEBUG) _cache.Keys.ToList().ForEach(nodeT => Console.WriteLine($"{(nodeT)} -> {_cache[(nodeT)]}"));
		
		// part 2
		complexity = 0;
		for (int i = 4; i <= 26; i++) // run for every length to fully utilize cache
		{
			foreach (string line in input)
			{
				long fullSeq = GetFullRobotSequence(line, i);
				if (i == 26) complexity += fullSeq * long.Parse(line.Substring(0, 3));
			}
		}
		Console.WriteLine($"Part 2: {complexity}");
	}

	private long GetFullRobotSequence(string code, int nrRobots) => GetRobotSequence(code, nrRobots, 0);
 
    
	private long GetRobotSequence(string pattern, int nrRobots, int robot)
	{
		if (robot == nrRobots - 1) return GetFinalRobotSequence(pattern, nrRobots, robot);

		if (robot > 0 && _cache.ContainsKey((pattern, nrRobots-robot))) {
			long cached = _cache[(pattern, nrRobots-robot)];
			if (DEBUG) Console.WriteLine($"CACHE get {pattern}, {nrRobots-robot}, {cached}");
			return cached;
		}

		List<long> finalInputSeqs = new List<long>(){0};
		Node prevNode = (robot == 0 ? _doorGraph : _robotGraph).Find(n => n.id == 'A');
		foreach (char c in pattern) {
			finalInputSeqs = finalInputSeqs.SelectMany(prevSeq => GetRobotSequence(c, prevSeq, prevNode, nrRobots, robot)).ToList();
			prevNode = (robot == 0 ? _doorGraph : _robotGraph).Find(n => n.id == c);
		}
		long result = finalInputSeqs.Min();
        
		if (robot > 0 && !_cache.ContainsKey((pattern, nrRobots-robot))) { 
			TryAddToCache(pattern, robot, nrRobots, result);
		}
        
		return result;
	}
    
	private List<long> GetRobotSequence(char targetOutputKey, long prevLength, Node prevNode, int nrRobots, int robot)
	{
		if (robot == nrRobots - 1) return GetFinalRobotSequence(targetOutputKey, prevNode, nrRobots).Select(l => prevLength + l).ToList();

		List<long> finalInputSeqs = new List<long>();

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
			string nextRobotPathString = "";
			Node prevPos = prevNode;
			foreach (Node step in path)
			{
				nextRobotPathString += GetNextInputKey(step.pos, prevPos.pos);
				prevPos = step;
			}
			finalInputSeqs.Add(GetRobotSequence(nextRobotPathString+"A", nrRobots, robot+1));
		}
		
		if (finalInputSeqs.Count == 0)
		{
			prevNode = _robotGraph.Find(n => n.id == 'A');
			finalInputSeqs.AddRange(GetRobotSequence('A', 0, prevNode, nrRobots, robot+1));
		}

		return finalInputSeqs.Select(l => prevLength + l).ToList();
	}

	private void TryAddToCache(string fromChar, int robot, int nrRobots, long newLength)
	{
		if (DEBUG) Console.WriteLine($"CACHE ADD {fromChar}, {nrRobots-robot}, {newLength}");
		_cache[(fromChar, nrRobots - robot)] = newLength;
	}

	private long GetFinalRobotSequence(string pattern, int nrRobots, int robot)
	{
		long result = 0;
		Node prevNode = (robot == 0 ? _doorGraph : _robotGraph).Find(n => n.id == 'A')!;
		foreach (char c in pattern)
		{
			result += GetFinalRobotSequence(c, prevNode, nrRobots).Min();
			prevNode = (robot == 0 ? _doorGraph : _robotGraph).Find(n => n.id == c)!;
		}
		return result;
	}
    
	private List<long> GetFinalRobotSequence(char targetOutKey, Node prevNode, int nrRobots)
	{
		// find targetKeyboardNode
		var targetKeyboardGraph = nrRobots == 1 ? _doorGraph : _robotGraph;
		var pathLengths = nrRobots == 1 ? _doorPathLengths : _robotPathLengths;
        
		Node endNode = targetKeyboardGraph.Find(n => n.id == targetOutKey)!;
		List<List<Node>>? paths = pathLengths[(prevNode, endNode)]; // paths to simulate (on previous keyboard)
        
		// handle multiple (output) paths
		List<long> thisRobotInputs = new List<long>();
		foreach (List<Node> path in paths ?? Enumerable.Empty<List<Node>>())
		{
			string t ="";
			Node prevPos = prevNode;
			foreach (Node step in path)
			{
				char newTargetKey = GetNextInputKey(step.pos, prevPos.pos);
				prevPos = step;
				t += newTargetKey;
			}
			thisRobotInputs.Add(t.Length);
		}
        
		if (thisRobotInputs.Count == 0) thisRobotInputs.Add(0);

		// finally press A (on input) to confirm this key
		for (int i = 0; i < thisRobotInputs.Count; i++)
		{
			thisRobotInputs[i]++;  // += 'A';
		}

		return thisRobotInputs;
	}
	
	private char GetNextInputKey(Vector2Int newPos, Vector2Int oldPos)
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