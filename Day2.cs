using System.Text.RegularExpressions;

namespace AoC2024;

public class Day2
{
	public void Run(List<string> lines)
	{
		List<List<int>> values = lines.Select(l => Regex.Matches(l, "[0-9]+").Select(m => Int32.Parse(m.Value)).ToList()).ToList();
		
		int part1 = values.Count(IsSafe);
		Console.WriteLine($"PART 1: {part1}");
		
		int part2 = values.Count(CanBeSafe);
		Console.WriteLine($"PART 2: {part2}");
	}

	bool CanBeSafe(List<int> line)
	{
		if (IsSafe(line)) return true;
		// exhaustively, just try removing every value (with costly remove/insert)
		for (int i = 0; i < line.Count; i++)  {
			int nr = line[i];
			line.RemoveAt(i);
			if (IsSafe(line)) return true;
			line.Insert(i, nr);
		}
		return false;
	}
	
	bool IsSafe(List<int> line)
	{
		bool up = line[1] > line[0];
		for (int i = 0; i < line.Count-1; i++)
		{
			int diff = line[i+1] - line[i];
			if ((up && diff < 0) || (!up && diff > 0) || Math.Abs(diff) > 3 || Math.Abs(diff) < 1) return false;
		}
		return true;
	}
}