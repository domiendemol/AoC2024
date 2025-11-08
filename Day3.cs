using System.Text.RegularExpressions;

namespace AoC2024;

public class Day3
{
	public (string, string) Run(List<string> lines)
	{
		var matches = lines.Select(line => Regex.Matches(line, $"mul\\(([0-9]+),([0-9]+)\\)"));
		long part1 = matches.Sum(mc => mc.Sum(m => Int32.Parse(m.Groups[1].Value) * Int32.Parse(m.Groups[2].Value)));

		// part 2
		// DO NOT TREAT THEM AS SEPARATE LINES
		// turned out it was one long instruction list :/
		string merged = string.Join(" ", lines.ToArray()); 
		// split by "don't", then by "do()", and count mul()s
		long sum = GetTotal(merged.Split("don't()").ToList());
		
		return (part1.ToString(), sum.ToString());
	}

	private long GetTotal(List<string> parts)
	{		
		// first part is good!, count multiplications
		List<Match> first = Regex.Matches(parts.First(), $"mul\\(([0-9]+),([0-9]+)\\)").ToList();
		parts.RemoveAt(0);
		
		// split rest by "do()"
		List<string> doParts = parts.Select(p => p.Split("do()").Where((s, i) => i > 0)).SelectMany(x => x).ToList();
		// every part except the first is guaranteed good, count all multiplications
		List<MatchCollection> mul = doParts.Select(line => Regex.Matches(line, $"mul\\(([0-9]+),([0-9]+)\\)")).ToList();
		
		return first.Sum(m => Int64.Parse(m.Groups[1].Value) * Int64.Parse(m.Groups[2].Value))
					+ mul.Sum(mc => mc.Sum(m => Int64.Parse(m.Groups[1].Value) * Int64.Parse(m.Groups[2].Value)));
	}
	
	private  static void PrintDebug(List<string> lines)
	{
		MatchCollection mc = Regex.Matches(lines[0], $"mul{Regex.Escape("(")}([0-9]+),([0-9]+){Regex.Escape(")")}");
		foreach (Match match in mc) {
			Console.Write(match.Groups[0].Value);
			Console.Write("  --  "+match.Groups[1].Value);
			Console.Write(" * "+match.Groups[2].Value);
			Console.WriteLine(" = "+Int64.Parse(match.Groups[1].Value) * Int64.Parse(match.Groups[2].Value));
		}
	}
}