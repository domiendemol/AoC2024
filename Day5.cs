using System.Text.RegularExpressions;

namespace AoC2024;

public class Day5
{
	private Dictionary<int, List<int>> _after = new Dictionary<int, List<int>>();
	private Dictionary<int, List<int>> _before = new Dictionary<int, List<int>>();
	
	public void Run(List<string> input)
	{
		// parse input and store rules in _after/_before
		var rules = input.Select(line => Regex.Matches(line, @$"([0-9]+)\|([0-9]+)"));
		rules.Where(mc => mc.Count > 0).Select(mc => (Int32.Parse(mc[0].Groups[1].Value), Int32.Parse(mc[0].Groups[2].Value))).
			ToList().ForEach(r => AddToList(r.Item1, r.Item2));

		int part1 = input.Where(line => Regex.Match(line, ",").Success).Select(line => line.Split(",")).
			Where(a => a.Length > 0).Where(pages => IsSectionOrdered(pages)).
			Sum(r => Convert.ToInt32(r[r.Length/2]));
		Console.WriteLine($"Part 1: {part1}");
		
		// part 2, order them 
		int part2 = input.Where(line => Regex.Match(line, ",").Success).Select(line => line.Split(",")).
			Where(a => a.Length > 0).Where(pages => !IsSectionOrdered(pages)).
			Select(pages => FixOrdering(pages)).Sum(p => p[p.Length/2]);
		Console.WriteLine($"Part 2: {part2}");
	}

	private int[] FixOrdering(string[] pageStrings)
	{
		int[] pages = Array.ConvertAll(pageStrings, s => int.Parse(s));
		for (int i = 0; i < pages.Length-1; i++)
		{
			int page = pages[i];
			for (int j = i+1; j < pages.Length; j++)
			{
				int page2 = pages[j];
				if ((_after.ContainsKey(page) && !_after[page].Contains(page2)) || (_before.ContainsKey(page2) && !_before[page2].Contains(page))) {
					pages[i] = page2;
					pages[j] = page;
					page = page2;
					j = i + 1;
				}
			}
		}
		// Console.WriteLine("GOOD: " + Convert.ToInt32(pages[pages.Length/2]));
		return pages;
	}

	private bool IsSectionOrdered(string[] pageStrings)
	{
		int[] pages = Array.ConvertAll(pageStrings, s => int.Parse(s));
		for (int i = 0; i < pages.Length-1; i++)
		{
			int page = pages[i];
			for (int j = i+1; j < pages.Length; j++)
			{
				int page2 = pages[j];
				if (_after.ContainsKey(page) && !_after[page].Contains(page2)) return false;
				if (_before.ContainsKey(page2) && !_before[page2].Contains(page)) return false;
			}
		}
		// Console.WriteLine("GOOD: " + Convert.ToInt32(pages[pages.Length/2]));
		return true;
	}
	
	private void AddToList(int first, int second)
	{
		if (!_after.ContainsKey(first)) _after[first] = new List<int>();
		_after[first].Add(second);
		if (!_before.ContainsKey(second)) _before[second] = new List<int>();
		_before[second].Add(first);
	}
}