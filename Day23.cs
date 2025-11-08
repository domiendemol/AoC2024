namespace AoC2024;

public class Day23
{
	struct Computer : IEquatable<Computer>
	{
		public string name;
		public HashSet<Computer> siblings = new HashSet<Computer>();

		public Computer() {
			name = null;
		}
		public override string ToString() => name;
		public bool Equals(Computer other) => name == other.name;
		public override bool Equals(object? obj) => obj is Computer other && Equals(other);
		public override int GetHashCode() => name.GetHashCode();
	}
			
	public (string, string) Run(List<string> input)
	{
		List<(string, string)> connections = input.Select(line => (line.Split('-')[0], line.Split('-')[1])).ToList();
		Dictionary<string, Computer> computers = new Dictionary<string, Computer>();
		List<(Computer, Computer)> computerConnections = connections.Select(conn => (GetOrAddComputer(conn.Item1, computers), GetOrAddComputer(conn.Item2, computers))).ToList();
		
		// group computers in sets
		List<HashSet<Computer>> groups = new List<HashSet<Computer>>();
		computerConnections.ForEach(conn => Connect(groups, conn.Item1, conn.Item2));

		// split larger groups in 3-element lists, and remove duplicates
		List<List<Computer>> threeGroups = SplitIn3(groups);
		List<List<Computer>> uniqueGroups = threeGroups.Select(set => set.ToList().OrderBy(c => c.name).ToList()).DistinctBy(l => string.Join(',', l)).ToList(); 

		// Part 1
		int part1 = uniqueGroups.Count(group => group.Count == 3 && group.Any(c => c.name.StartsWith('t')));
		Console.WriteLine($"Part 1: {part1}");
		
		// Part 2
		HashSet<Computer> largest = groups.MaxBy(l => l.Count());
		string part2 = string.Join(',', largest.OrderBy(c => c.name).Select(l => string.Join(',', l)));
		
		return (part1.ToString(), part2);
	}

	private Computer GetOrAddComputer(string computerName, Dictionary<string, Computer> computers)
	{
		if (computers.ContainsKey(computerName)) return computers[computerName];
		Computer computer = new Computer(){name = computerName};
		computers.Add(computerName, computer);
		return computer;
	}

	private List<List<Computer>> SplitIn3(List<HashSet<Computer>> listOfLists)
	{
		List<List<Computer>> transformedList = new List<List<Computer>>();
		foreach (var sublist in listOfLists)
		{
			if (sublist.Count > 3) {
				transformedList.AddRange(GetCombinations(sublist.ToList(), 3));
			}
			else if (sublist.Count == 3) {
				transformedList.Add(sublist.ToList());
			}
		}
		
		return transformedList;
	}
	
	// Method to generate combinations of a specific size
	public static List<List<T>> GetCombinations<T>(List<T> list, int combinationSize)
	{
		List<List<T>> combinations = new List<List<T>>();

		void Combine(List<T> current, int index)
		{
			if (current.Count == combinationSize)
			{
				combinations.Add(new List<T>(current));
				return;
			}

			for (int i = index; i < list.Count; i++)
			{
				current.Add(list[i]);
				Combine(current, i + 1);
				current.RemoveAt(current.Count - 1); // Backtrack
			}
		}

		Combine(new List<T>(), 0);
		return combinations;
	}

	private void Connect(List<HashSet<Computer>> groups, Computer computer1, Computer computer2)
	{
		computer1.siblings.Add(computer2);
		computer2.siblings.Add(computer1);
		
		HashSet<Computer> toAddGroup = null;
		foreach (var group in groups) 
		{
			// all computers must be siblings of these two
			if (group.All(c => (c.Equals(computer1) && c.siblings.Contains(computer2)) ||
										(c.Equals(computer2) && c.siblings.Contains(computer1)) ||
										c.siblings.Contains(computer1) && c.siblings.Contains(computer2))) {
				group.Add(computer1);
				group.Add(computer2);
			}
		}
		if (toAddGroup == null) {
			toAddGroup = new HashSet<Computer>(){computer1, computer2};
			groups.Add(toAddGroup);
		}
	}
}