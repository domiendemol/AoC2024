namespace AoC2024;

public class Day23
{
	struct Computer
	{
		public string name;
		public HashSet<Computer> siblings = new HashSet<Computer>();

		public Computer() {
			name = null;
		}
		public override string ToString() => name;
	}
			
	public void Run(List<string> input)
	{
		List<(string, string)> connections = input.Select(line => (line.Split('-')[0], line.Split('-')[1])).ToList();
		HashSet<Computer> computers = new HashSet<Computer>();
		List<HashSet<Computer>> trueGroups = new List<HashSet<Computer>>();
		connections.ForEach(conn => Connect(computers, trueGroups, conn.Item1, conn.Item2));

		List<List<Computer>> threeGroups = SplitIn3(trueGroups);
		List<List<Computer>> uniqueGroups = threeGroups.Select(set => set.ToList().OrderBy(c => c.name).ToList()).DistinctBy(l => string.Join(',', l)).ToList(); 
		
		// uniqueGroups.ForEach(l => Console.WriteLine(string.Join(',', l)));
		int part1 = uniqueGroups.Count(group => group.Count == 3 && group.Any(c => c.name.StartsWith('t')));
		Console.WriteLine($"Part 1: {part1}");
		
		// Part 2
		HashSet<Computer> largest = trueGroups.MaxBy(l => l.Count());
		Console.WriteLine($"Part 2: {string.Join(',', largest.OrderBy(c => c.name).Select(l => string.Join(',', l)))}");
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

		// foreach (var sublist in transformedList) {
			// Console.WriteLine($"Sublist of {string.Join(',', sublist)}");
		// }
		
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

	private void Connect(HashSet<Computer> computers, List<HashSet<Computer>> groups, string computer1Name, string computer2Name)
	{
		Computer computer1 = computers.FirstOrDefault(c => c.name == computer1Name, new Computer(){name = computer1Name});
		Computer computer2 = computers.FirstOrDefault(c => c.name == computer2Name, new Computer(){name = computer2Name});
		
		computer1.siblings.Add(computer2);
		computer2.siblings.Add(computer1);
		computers.Add(computer1);
		computers.Add(computer2);
		
		HashSet<Computer> toAddGroup = null;
		foreach (var group in groups) 
		{
			// all computers must be siblings of these two
			if (group.All(c => (c.Equals(computer1) && c.siblings.Contains(computer2)) ||
										(c.Equals(computer2) && c.siblings.Contains(computer1)) ||
										c.siblings.Contains(computer1) && c.siblings.Contains(computer2)))
			{
				group.Add(computer1);
				group.Add(computer2);
			}
		}
		if (toAddGroup == null) {
			toAddGroup = new HashSet<Computer>();
			groups.Add(toAddGroup);
			toAddGroup.Add(computer1);
			toAddGroup.Add(computer2);
		}
	}
}