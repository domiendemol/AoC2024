using System.Text.RegularExpressions;

namespace AoC2024;

public class Day13
{
    struct Machine
    {
        public int ax, ay, bx, by;
        public long A, B;
        
        public double CalcA() => (B * bx - by * A) / (double) (ay * bx - by * ax); // a = (B*bx - by*A) / (ay*bx - by*ax)
        public double CalcB(long a) => (A - a * ax) / (double) bx;  // b = (A − a*ax) / bx
    }
    
    public (string, string) Run(List<string> input)
    {
        var numbers = input.Where(line => line.Length > 0).SelectMany(line => Regex.Matches(line, @"(\d+)")).ToList();
        List<Machine> machines = new List<Machine>();
        for (int i = 0; i < numbers.Count(); i+=6)
        {
            machines.Add(new Machine()
            {
                ax = Convert.ToInt32(numbers[i].Groups[0].Value),
                ay = Convert.ToInt32(numbers[i+1].Groups[0].Value),
                bx = Convert.ToInt32(numbers[i+2].Groups[0].Value),
                by = Convert.ToInt32(numbers[i+3].Groups[0].Value),
                A = Convert.ToInt32(numbers[i+4].Groups[0].Value),
                B = Convert.ToInt32(numbers[i+5].Groups[0].Value),
            });
        }
        
        // ak + bl = X, am + bn = Y
        // solves to ==> a = (YL - nX) / (ml - nk)
        // in our var names:
        // a = (B*bx - by*A) / (ay*bx - by*ax)   and    b = (A − a*ax) / bx
        
        return (machines.Sum(machine => GetTokens(machine)).ToString(),
                machines.Sum(machine => GetTokens(machine, 10000000000000)).ToString());
    }

    private long GetTokens(Machine machine, long toAdd = 0)
    {
        machine.A += toAdd;
        machine.B += toAdd;
        double aPresses = machine.CalcA();
        if (Math.Truncate(aPresses) != aPresses) return 0; // integer test

        return (long) (aPresses * 3 + machine.CalcB((long) aPresses));
    }
}