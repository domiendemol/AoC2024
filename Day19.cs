namespace AoC2024;

public class Day19
{
	private Dictionary<string, bool> _validComboCache = new();
	private Dictionary<string, long> _validComboCountCache = new();
	
	public void Run(List<string> input)
	{
		List<string> towels = input[0].Split(", ").Where(s => s.Trim().Length > 0).OrderBy(t => t.Length).Reverse().ToList();
		List<string> patterns = input.Skip(2).ToList();

		// Part 1
		// optimization: first remove all towels that are already composed of others
		List<string> singularTowels = new List<string>();
		foreach (var towel in towels)
		{
			bool composed = TryPattern(towel, towels.Where(t => !t.Equals(towel)).ToList());
			if (!composed) singularTowels.Add(towel);
		}
		singularTowels = singularTowels.OrderBy(t => t.Length).Reverse().ToList();
		
		List<string> validCombos = patterns.Where(p => TryPattern(p, singularTowels)).ToList(); 
		Console.WriteLine($"Part 1: {validCombos.Count}");
		
		// Part 2
		// use same recursive method but count ALL options (and add super important cache)
		Console.WriteLine($"Part 2: {validCombos.Sum(combo => GetPatternCount(combo, towels))}");
	}

	// recursive  
	private bool TryPattern(string pattern, List<string> towels)
	{
		foreach (var towel in towels)
		{
			// test if it matches so far, or in our cache
			if (towel == pattern) return true;
			if (towel.Length >= pattern.Length) continue; // too far, abort
			if (!PatternStartsWithTowel(towel, pattern)) continue;
			
			string next = pattern.Substring(towel.Length);
			if (_validComboCache.ContainsKey(next)) return true;
			
			// if so, continue
			bool match = TryPattern(next, towels);
			if (match)
			{
				_validComboCache[next] = true;
				return true;
			}
		}

		return false;
	}
	
	// like TryPattern but return the actual parts
	private bool GetPattern(string pattern, List<string> towels, List<string> parts)
	{
		foreach (var towel in towels)
		{
			// test if it matches so far, or in our cache
			if (towel == pattern) {
				parts.Add(towel); 
				return true;
			}
			if (towel.Length >= pattern.Length) continue; // too far, abort
			if (!PatternStartsWithTowel(towel, pattern)) continue;
			
			string next = pattern.Substring(towel.Length);
			
			// if so, continue
			bool match = GetPattern(next, towels, parts);
			if (match)
			{
				_validComboCache[towel] = true;
				parts.Add(towel);
				return true;
			}
		}

		return false;
	}
	
	private long GetPatternCount(string pattern, List<string> towels)
	{
		if (_validComboCountCache.ContainsKey(pattern)) return _validComboCountCache[pattern];
		
		long count = 0;
		foreach (var towel in towels)
		{
			// test if it matches so far, or in our cache
			if (towel == pattern) {
				count++;
				continue;
			}
			if (towel.Length >= pattern.Length) continue; // too far, abort
			if (!PatternStartsWithTowel(towel, pattern)) continue;
			
			string next = pattern.Substring(towel.Length);
			
			// if so, continue
			if (_validComboCountCache.TryGetValue(next, out long cc)) 
				count += cc;
			else
			{
				long c = GetPatternCount(next, towels);
				if (c > 0) {
					count += c;
				}
			}
		}

		if (_validComboCountCache.ContainsKey(pattern)) _validComboCountCache[pattern] = count;
		else _validComboCountCache[pattern] = count;
		return count;
	}


	private bool PatternStartsWithTowel(string towel, string pattern)
	{
		for (int i = 0; i < towel.Length; i++)
		{
			if (towel[i] != pattern[i]) return false;
		}
		return true;
	}
}