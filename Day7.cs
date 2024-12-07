namespace AoC2024;

public class Day7
{ 
    public void Run(List<string> input)
    {
        List<List<long>> equations = input.Select(line => line.Split(' ').Select((val, i) => Convert.ToInt64(i == 0 ? val.Substring(0,val.Length-1) : val)).ToList()).ToList();
        
        long part1 = equations.Sum(eq => CalcPart(eq, eq[1], 2, false));
        Console.WriteLine($"Part 1: {part1}");
        
        long part2 = equations.Sum(eq => CalcPart(eq, eq[1], 2, true));
        Console.WriteLine($"Part 2: {part2}");
    }

    private long CalcPart(List<long> equation, long currTotal, int index, bool concat) 
    {
        long result1 = currTotal + equation[index];
        long result2 = currTotal * equation[index];
        long result3 = concat ? Convert.ToInt64(Convert.ToString(currTotal) + equation[index]) : 0;
        if (index == equation.Count - 1) {   // last part, check result
            return equation[0] == result1 ? result1 : (equation[0] == result2 ? result2 : (equation[0] == result3 ? result3 : 0));
        }
        
        long next1 = CalcPart(equation, result1, index + 1, concat);
        if (next1 != 0) return next1;
        long next2 = CalcPart(equation, result2, index + 1, concat);
        if (next2 != 0) return next2;
        long next3 = concat ? CalcPart(equation, result3, index + 1, concat) : 0;
        if (next3 != 0) return next3;
        return next1 != 0 ? next1 : (next2  != 0 ? next2 : (next3 != 0 ? next3 : 0));
    }
}