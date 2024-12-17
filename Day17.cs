using System.Text.RegularExpressions;

namespace AoC2024;

public class Day17
{
    private const bool BRUTE = true;
    
    private int[] _instructions;
    private int[] _operands;
    
    public void Run(List<string> input)
    {
        // read 3 registers and program
        int[] register = new int[3];
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
        Console.WriteLine($"Program: {string.Join(',', program)}");
        
        // Part 2
        if (BRUTE)
        {
            int test = 0;
            int c1 = 0;
            int cycle = 0;
            List<int> output = null;
            string expected = string.Join(',', program);
            while (output == null || !string.Join(',', output).Equals(expected))
            {
                register[0] = ++test;
                register[1] = 0;
                register[2] = 0;
                output = ExecuteProgram(register);
                // Console.WriteLine(string.Join(',', output) + " - " + test);
                if (output.Count == 2 && output[0] == 1 && output.Sum() == 1 && c1 == 0)
                {
                    Console.WriteLine(string.Join(',', output) + " - " + test);
                    c1 = test;
                }
                if (output.Count == 2 && output[0] == 2 && output.Sum() == 2 && c1 > 0)
                {
                    Console.WriteLine(string.Join(',', output) + " - " + test);
                    cycle = test - c1;
                    //break;
                }
                if (output.Count >= 2 && output.Sum() == 0)
                {
                    Console.WriteLine(string.Join(',', output) + " - " + test);
                    //break;
                }
                Console.WriteLine(string.Join(',', output) + " - " + test);
                if (test == 400) break;
            }
            
            Console.WriteLine(cycle);
            cycle = 8;
            // count cycle per program/output digit
            long part2 = 0;
            for (int i = 0; i < _instructions.Length; i++)
            {
                part2 += (long) (_instructions[i] * Math.Pow(cycle, (i*2)+1));
                Console.WriteLine($"Part 2: {part2}");
                part2 += (long) (_operands[i] * Math.Pow(cycle, (i*2)+2));
                Console.WriteLine($"Part 2: {part2}");
            }
            
            // Console.WriteLine($"Register: {string.Join(',', register)}");
            Console.WriteLine($"Part 2: {test}");
            Console.WriteLine($"Part 2: {part2}");
        }
        // count cycle of going from 0, to 1, 
        else
        {
            // REverse?
        }
    }

    // execute program on given register set
    private List<int> ExecuteProgram(int[] register)
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
    private int ExecuteInstruction(int ip, int[] register, ref List<int> outputBuffer)
    {
        // Console.WriteLine($"Executing at pointer: {ip}, inst: {_instructions[ip]} ({_operands[ip]}/{ComboOperand(ip, register)})");
        int prevIp = ip;
        switch (_instructions[ip])
        {
            case 0: // adv
                register[0] = (int) (register[0] / Math.Pow(2, ComboOperand(ip, register)));
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
                outputBuffer.Add(ComboOperand(ip, register) % 8);
                break;
            case 6: // bdv
                register[1] = (int) (register[0] / Math.Pow(2, ComboOperand(ip, register)));
                break;
            case 7: // cdv
                register[2] = (int) (register[0] / Math.Pow(2, ComboOperand(ip, register)));
                break;
        }
        
        // Console.WriteLine("==>"+string.Join(',', register));
        
        if (prevIp == ip) ip++;
        return ip;
    }

    private int ComboOperand(int ip, int[] register)
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
    
    private string ReverseExecuteProgram(int[] register)
    {
        int ip = 7; // we start at la
        List<int> outputBuffer = new List<int>();
        while (ip < _instructions.Length)
        {
            ip = ExecuteInstruction(ip, register, ref outputBuffer);
        }
        
        Console.WriteLine(string.Join(',', outputBuffer));
        return (string.Join(',', outputBuffer));
    }
    
    private int ReverseExecuteInstruction(int ip, int[] register, ref List<int> outputBuffer)
    {
        // Console.WriteLine($"Executing at pointer: {ip}, inst: {_instructions[ip]} ({_operands[ip]}/{ComboOperand(ip, register)})");
        int prevIp = ip;
        switch (_instructions[ip])
        {
            case 0: // adv
                register[0] = (int) (register[0] * Math.Pow(2, ComboOperand(ip, register)));
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
                outputBuffer.Add(ComboOperand(ip, register) % 8); // ComboOperand(ip, register) must be dividle by 8
                break;
            case 6: // bdv
                register[1] = (int) (register[0] * Math.Pow(2, ComboOperand(ip, register)));
                break;
            case 7: // cdv
                // register[2] = (int) (register[0] / Math.Pow(2, ComboOperand(ip, register)));
                register[2] = (int) (register[0] * Math.Pow(2, ComboOperand(ip, register)));
                break;
        }
        
        // Console.WriteLine("==>"+string.Join(',', register));
        
        if (prevIp == ip) ip--;
        return ip;
    }
}