using System.ComponentModel;
using System.Text.RegularExpressions;

namespace AoC2024;

public class Day24
{
    public void Run(List<string> input)
    {
        Dictionary<string, int> wires = new();
        List<Gate> gates = new();
        
        input.Where(line => line.Contains(":")).Select(line => line.Split(": ")).ToList().
            ForEach(i => wires.Add(i[0], int.Parse(i[1])));
        input.SelectMany(line => Regex.Matches(line, @$"^([a-z0-9]+) ([A-Z]+) ([a-z0-9]+) -> ([a-z0-9]+)")).ToList().
            ForEach(m => gates.Add(new Gate(){op = m.Groups[2].Value, in1 = m.Groups[1].Value, in2 = m.Groups[3].Value, outWire = m.Groups[4].Value}));
        
        // Part 1
        ExecAllGates(wires, ref gates);
        string binaryString = string.Join("", wires.Where(w => w.Key.StartsWith("z")).OrderBy(kvp => kvp.Key).Reverse().Select(kvp => kvp.Value));
        // Console.WriteLine(binaryString);
        Console.WriteLine($"Part 1: {Convert.ToInt64(binaryString,2)}");
        
        Part2(wires, gates);
    }
    
    private void Part2(Dictionary<string, int> wires, List<Gate> gates, bool debug = false)
    {
        List<Gate> andGates = gates.Where(g => g.op == "AND").ToList();
        List<Gate> xorGates = gates.Where(g => g.op == "XOR").ToList();
        List<Gate> orGates = gates.Where(g => g.op == "OR").ToList();
        List<string> finalWrongWires = new List<string>();
        
        // check a bunch of rules, to be valid combination of adders
       
        // output of AND must be input of OR
        List<string> kaputWires = andGates.Select(g => g.outWire).Where(outWire => orGates.All(g=> g.in1!=outWire && g.in2!=outWire)).ToList();
        kaputWires = kaputWires.Where(w => !w.StartsWith('z')).ToList();
        kaputWires = kaputWires.Where(w => w != FindGateWithInputs(andGates, "x00","y00").outWire).ToList(); // exclude first adder
        finalWrongWires.AddRange(kaputWires);
        if (debug) Console.WriteLine($"1: {string.Join(", ", kaputWires)}");
        
        // input of AND must be input of XOR
        List<string> kaputWires2 = andGates.Select(g => g.in1).Where(inWire => xorGates.All(g=> g.in1!=inWire && g.in2!=inWire)).
                            Union(andGates.Select(g => g.in2).Where(inWire => xorGates.All(g=> g.in1!=inWire && g.in2!=inWire))).ToList();
        kaputWires2 = kaputWires2.Where(w => !w.StartsWith('x') && !w.StartsWith('y') && !w.StartsWith('z')).ToList();
        finalWrongWires.AddRange(kaputWires2);
        if (debug) Console.WriteLine($"2: {string.Join(", ", kaputWires2)}");
        
        // inputs of XOR must also be inputs of same AND
        List<(string, string)> kaputWires3 = xorGates.Select(g => (g.in1, g.in2)).Where(ins => andGates.All(g=> g.in1!=ins.Item1 && g.in2!=ins.Item2 && g.in2!=ins.Item1 && g.in1!=ins.Item2)).ToList();
        if (debug) Console.WriteLine($"3: {string.Join(", ", kaputWires3)}");
        
        // every z wire must be output of XOR  
        List<string> kaputWires4 = wires.Select(kvp=>kvp.Key).Where(z => z.StartsWith("z")).Where(z => !xorGates.Any(g => g.outWire == z)).ToList();
        finalWrongWires.AddRange(kaputWires4.Where(w => w!="z45")); // exclude last one
        if (debug) Console.WriteLine($"4: {string.Join(", ", kaputWires4)}");
 
        // input of XOR must be output of XOR, if not true, it's a cIN
        List<string> cInWires = xorGates.Select(g => g.in1).Where(inWire => !xorGates.Any(g=> g.outWire==inWire)).
            Union(xorGates.Select(g => g.in2).Where(inWire => !xorGates.Any(g=> g.outWire==inWire))).ToList();
        cInWires = cInWires.Where(w => !w.StartsWith('x') && !w.StartsWith('y') && !w.StartsWith('z')).ToList();
        // cIN must be input of AND
        List<string> kaputWires5 = cInWires.Where(inWire => andGates.All(g => g.in1 != inWire && g.in2 != inWire)).ToList();
        finalWrongWires.AddRange(kaputWires5);
        if (debug) Console.WriteLine($"5: {string.Join(", ", kaputWires5)}");

        // or can we follow actual inputs, let's say x01 and y01
        // get XOR output
        // get AND output
        // XOR output must lead into AND input 1
        // finally, both AND outputs must lead into final OR

        List<Gate> inputXors = xorGates.Where(xor => IsOrigInput(xor.in1) || IsOrigInput(xor.in2)).ToList();
        // each xor must have matching AND (same inputs)
        foreach (Gate xorGate in inputXors)
        {
            string xorOutput = xorGate.outWire;
            // top AND (after XOR)
            Gate topAnd = andGates.Find(and => and.in1 == xorOutput || and.in2 == xorOutput);
            if (topAnd == null)
            {
                if (xorOutput != "z00") finalWrongWires.Add(xorOutput);
                if (debug) Console.WriteLine($"xor without and: {xorOutput}");
            }
            else
            {
                string andOutput = topAnd.outWire;
                // next this AND output must go into OR
                Gate nextorG = FindGateWithInput(orGates, andOutput);
                if (nextorG == null)
                {
                    if (debug) Console.WriteLine($"WEEEUUUUUWEEEEUUU top AND not to OR: {andOutput}");
                }
                else
                {
                    // now get the AND after the actual inputs
                    Gate? btmAnd = FindGateWithInputs(andGates, xorGate.in1, xorGate.in2);
                    if (btmAnd == null)
                    {
                        if (debug) Console.WriteLine($"WEEEUUUUUWEEEEUUU bottom AND not found: {xorOutput}");
                    }
                    else
                    {
                        // this AND must lead into same OR
                        Gate finalOr = FindGateWithInputs(orGates, btmAnd.outWire, topAnd.outWire);
                        if (finalOr == null)
                        {
                            if (debug) Console.WriteLine($"TUTATUTATUTATAUTA bottom and top AND don't end up in same OR {btmAnd.outWire}");
                            // get other input of OR
                            if (debug) Console.WriteLine(nextorG.in1);
                            if (debug) Console.WriteLine(nextorG.in2);
                        }
                    }
                }
            }

            Gate xorGate2 = FindGateWithInput(xorGates, xorOutput);
            if (xorGate2 == null)
            {
                if (debug) Console.WriteLine($"Second Xor not found: {xorOutput}");
                if (xorOutput != "z00") finalWrongWires.Add(xorOutput);
            }
            else
            {
                // this must lead into nothing (must be a z number)
                string xorOutput2 = xorGate2.outWire;
                Gate xorGateNextAdder = FindGateWithInput(gates, xorOutput2);
                if (xorGateNextAdder != null)
                {
                    if (debug) Console.WriteLine($"Next adder Xor found (but should be none): {xorOutput2}");
                    finalWrongWires.Add(xorOutput2);
                }
            }
        }
        
        // exclude z00
        finalWrongWires = finalWrongWires.Distinct().ToList();
        finalWrongWires.Sort();
        Console.WriteLine($"Part 2: {string.Join(",", finalWrongWires)}");
        // Some manual work was necessary to reveal ksv/z06, tqq/z20, z39/ckb, and kbs/nbd!! So: ckb,kbs,ksv,nbd,tqq,z06,z20,z39
        // tweaked the algorithm based on that 
    }

    private Gate FindGateWithInput(List<Gate> gates, string wireIn1) => gates.Find(g => g.in1 == wireIn1 || g.in2 == wireIn1);
    
    private Gate FindGateWithInputs(List<Gate> gates, string wireIn1, string wireIn2)
    {
        return gates.Find(g =>
            (g.in1 == wireIn1 && g.in2 == wireIn2) ||
            (g.in1 == wireIn2 && g.in2 == wireIn1));
    }

    private Gate FindGateWithOutput(List<Gate> gates, string wire) => gates.Find(g => g.outWire == wire);
    
    private bool IsOrigInput(string wire) => wire.StartsWith('x') || wire.StartsWith('y');
    
    private void ExecAllGates(Dictionary<string, int> wires, ref List<Gate> gates)
    {
        List<Gate> processedGates = new();
        for (int i = 0; i < gates.Count; i++)
        {
            Gate gate = gates[i];
            // check inputs, must be set
            if (wires.TryGetValue(gate.in1, out int in1Val) && wires.TryGetValue(gate.in2, out int in2Val))
            {
                int outVal = gate.Exec(in1Val, in2Val);
                wires[gate.outWire] = outVal;

                gates.Remove(gate);
                processedGates.Add(gate);
                i--;
            }

            if (i + 1 >= gates.Count) i = -1;
        }
        gates = processedGates;
    }
    
    // ugly (not correct and way too slow) attempt to loop all permutations of 4 wire switches based on faulty wires we found so far
    // turns out z45 was fine :(
    private void Part2BF(Dictionary<string, int> wires, List<Gate> gates)
    {
        List<string> faultyWires = new List<string>() { "z06", "z20", "z39", "z45"}; // , "kbs", "nbd"};
        Gate[] faultyGates = new Gate[] { FindGateWithOutput(gates, "z06"), FindGateWithOutput(gates, "z20"), 
            FindGateWithOutput(gates, "z39"), FindGateWithOutput(gates, "z45")
        };
        List<string> otherWires = wires.Keys.Where(w => !faultyWires.Contains(w) && !w.StartsWith('x') && !w.StartsWith('y')).Distinct().ToList();
        
        // build permutations
        for (int i = 0; i < otherWires.Count; i++) {
            for (int j = 0; j < otherWires.Count; j++) {
                for (int k = 0; k < otherWires.Count; k++) {
                    for (int l = 0; l < otherWires.Count; l++) {
                        if (i == j || i == k || i == l || j == k || j == l || k == l) continue;
                        // loop permutations and execute them
                        Gate[] origGates = new Gate[faultyGates.Length];
                        string[] permutation = new[] {otherWires[i], otherWires[j], otherWires[k], otherWires[l]};
                        // swap
                        for (int mm = 0; mm < permutation.Length; mm++) {
                            origGates[mm] = FindGateWithOutput(gates, permutation[mm]);
                            origGates[mm].outWire = faultyGates[mm].outWire;
                            faultyGates[mm].outWire = permutation[mm];
                        }

                        int validCounter = 0;
                        for (int nn = 0; nn < 3; nn++)
                        {
                            long x = new Random().NextInt64(4194303);
                            long y = new Random().NextInt64(4194303);
                            if (ValidateComputer(x, y, x+y, wires, ref gates)) validCounter++;
                            // if (j % 40 ==0 && nn == 2) Console.WriteLine($"{validCounter} -> {string.Join(',',permutation)}");s
                        }

                        if (validCounter > 0) {
                            Console.WriteLine($"{validCounter} -> {string.Join(',',permutation)}");
                            if (validCounter >= 5) return;
                        }
                        // swap back
                        for (int o = 0; o < permutation.Length; o++) {
                            origGates[o].outWire = permutation[o];
                            faultyGates[o].outWire = faultyWires[o];
                        }
                    }
                }
            }
        }
        Console.WriteLine("Permutations ready");
    }

    private bool ValidateComputer(long x, long y, long z, Dictionary<string, int> wires, ref List<Gate> gates)
    {
        string xBin = Convert.ToString(x, 2);
        string yBin = Convert.ToString(y, 2);
        for (int i = 0; i < 46; i++) {
            wires[$"x{i:00}"] = (i<xBin.Length) ? (int)(xBin[i] - '0') : 0;
            wires[$"y{i:00}"] = (i<yBin.Length) ? (int)(yBin[i] - '0') : 0;
        }
        ExecAllGates(wires, ref gates);
        
        string binaryString = string.Join("", wires.Where(w => w.Key.StartsWith("z")).OrderBy(kvp => kvp.Key).Reverse().Select(kvp => kvp.Value));
        long result = Convert.ToInt64(binaryString,2);
        return result == z;
    }

    class Gate
    {
        public string op;
        public string in1;
        public string in2;
        public string outWire;

        public int Exec(int in1, int in2)
        {
            switch (op)
            {
                case "AND":
                    return in1 & in2;
                case "OR":
                    return in1 | in2;
                case "XOR":
                    return in1 ^ in2;
            }
            return 0;
        }
    }
}