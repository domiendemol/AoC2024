using System.Text.RegularExpressions;

namespace AoC2024;

public class Day17
{
    private const bool BRUTE = false;
    
    private int[] _instructions;
    private int[] _operands;
    
    public void Run(List<string> input)
    {
        // read 3 registers and program
        long[] register = new long[3];
        register[0] = int.Parse(Regex.Match(input[0], "(\\d+)").Groups[1].Value);
        register[1] = int.Parse(Regex.Match(input[1], "(\\d+)").Groups[1].Value);
        register[2] = int.Parse(Regex.Match(input[2], "(\\d+)").Groups[1].Value);
        string[] program = input[4].Split(' ')[1].Split(',');
        _instructions = new int[program.Length / 2];
        _operands = new int[program.Length / 2];
        for (int i = 0; i < program.Length; i += 2)
        {
            _instructions[i / 2] = int.Parse(program[i]);
            _operands[i / 2] = int.Parse(program[i+1]);
        }
        
        Console.WriteLine($"Part 1: {string.Join(',', ExecuteProgram(register))}");
        
        // Part 2
        string expected = string.Join(',', program);
        if (BRUTE)
        {
            // First attempt, works for test input but not for the real one
            // because it doesn't start with a zero and I don't take the begin executions into account
            long a = 0;
            int c1 = 0;
            int cycle = 0;
            long prev = 0;
            List<int> output = null;
            while (output == null || !string.Join(',', output).Equals(expected))
            {
                register[0] = ++a;
                register[1] = 0;
                register[2] = 0;
                output = ExecuteProgram(register);
                // Console.WriteLine(string.Join(',', output) + " - " + test);
                if (output.Count == 2 && output[0] == 1 && output.Sum() == 1 && c1 == 0)
                {
                    Console.WriteLine(string.Join(',', output) + " - " + a);
                    // c1 = a;
                }

                bool match = true;
                for (int i = 0; i < output.Count; i++)
                {
                    if (output[i] != int.Parse(program[program.Length-output.Count +i])) match = false;
                }
                if (match)
                {
                    Console.WriteLine(string.Join(',', output) + " - " + a + " MATCH");
                    //a+=(long)Math.Pow(8, output.Count);
                    // a= a*8 - (1000*(output.Count-2));
                    a = a * 8l - 4*(a - prev);
                    prev = a;
                    //if (output.Count > 8) a *= 2;
                    //if (output.Count > 9) a *= 16;
                }
                //Console.WriteLine(string.Join(',', output) + " - " + a);
                if (output.Count == 16) break;
            }
            
            Console.WriteLine(cycle);
            cycle = 8;
            // count cycle per program/output digit
            long part2 = 0;
            for (int i = 0; i < _instructions.Length; i++)
            {
                part2 += (long) (_instructions[i] * Math.Pow(cycle, (i*2)));
                Console.WriteLine($"Part 2: {part2}");
                part2 += (long) (_operands[i] * Math.Pow(cycle, (i*2)+1));
                Console.WriteLine($"Part 2: {part2}");
            }
            
            // Console.WriteLine($"Register: {string.Join(',', register)}");
            Console.WriteLine($"Part 2: {a}");
            Console.WriteLine($"Part 2: {part2}");
        }
        else
        {
                //  Anyway, the approach was:
                //    write a function which essentially does the work of my input program (wrote a disassembler to reverse-engineer), but stops at a specified A_target instead of zero
                //    start at A_target = 0
                //    starting from the last digit of the program (the desired output) and working backwards, figure out which A = (single octal digit appended to A_target), when passed to our function, can output the current digit of the program
                //    add each of those possibilities to a list, and use the possibilities in the last iteration as A_targets for the next
                //    find min of the A's obtained in the final stage

                // check every number of our output, starting from the back
                List<long> queue = new List<long>(){0}; 
                for (int i = program.Length-1; i >= 0; i--)
                {
                    // Console.WriteLine($"Checking: {program[i]}");
                    List<long> prevA = new List<long>();
                    
                    // loop our previously found values (each possible option for the previous digit)
                    foreach (long a in queue)
                    {
                        // try every number til 8 (further not necessary)
                        for (int k = 0; k < 8; k++)
                        {
                            // queue nr times 8 (cause algo divides by 8 each time) + k
                            long aCur = (a * 8) + k;
                            register[0] = aCur;
                            List<int> output = ExecuteProgram(register);
                            if (output[0] == Int32.Parse(program[i])) {
                                // yes, this a matches -> store it
                                prevA.Add(aCur);
                                // Console.WriteLine(string.Join(',', output) + " - " + aCur + " MATCH");
                            }
                        }
                    }

                    queue = prevA; // use new found values as next to check
                }
                
                Console.WriteLine($"Part 2: {queue[0]}");
        }
    }

    private void TestDigit(int index)
    {
        for (int i = 0; i < 7; i++)
        {
            long[] reg = new long[3];
            ExecuteProgram(reg);
        }
    }

    // execute program on given register set
    private List<int> ExecuteProgram(long[] register)
    {
        int ip = 0;
        List<int> outputBuffer = new List<int>();
        while (ip < _instructions.Length)
        {
            ip = ExecuteInstruction(ip, register, ref outputBuffer);
        }
        
        return outputBuffer;
    }

    // returns new instruction pointer
    private int ExecuteInstruction(int ip, long[] register, ref List<int> outputBuffer)
    {
        // Console.WriteLine($"Executing at pointer: {ip}, inst: {_instructions[ip]} ({_operands[ip]}/{ComboOperand(ip, register)})");
        int prevIp = ip;
        switch (_instructions[ip])
        {
            case 0: // adv
                register[0] = (long) (register[0] / Math.Pow(2, ComboOperand(ip, register)));
                break;
            case 1: // bxl
                register[1] = register[1] ^ _operands[ip];
                break;
            case 2: // bst
                register[1] = ComboOperand(ip, register) % 8;
                break;
            case 3: // jnz
                if (register[0] != 0) ip = _operands[ip];
                break;
            case 4: // bxc
                register[1] = register[1] ^ register[2];
                break;
            case 5: // out
                outputBuffer.Add((int) (ComboOperand(ip, register) % 8));
                break;
            case 6: // bdv
                register[1] = (long) (register[0] / Math.Pow(2, ComboOperand(ip, register)));
                break;
            case 7: // cdv
                register[2] = (long) (register[0] / Math.Pow(2, ComboOperand(ip, register)));
                break;
        }
        
        // Console.WriteLine("==>"+string.Join(',', register));
        
        if (prevIp == ip) ip++;
        return ip;
    }

    private long ComboOperand(int ip, long[] register)
    {
        // Combo operands 0 through 3 represent literal values 0 through 3.
        // Combo operand 4 represents the value of register A.
        // Combo operand 5 represents the value of register B.
        // Combo operand 6 represents the value of register C.
        // Combo operand 7 is reserved and will not appear in valid programs.
        int val = _operands[ip];
        if (val <= 3) return val;
        if (val == 7) throw new Exception("Invalid combo operand value 7");
        return register[val-4];
    }
    
}