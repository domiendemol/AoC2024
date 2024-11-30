using System.Text;

namespace AoC2024;

public static class Utils
{
    public static string ReplaceAtIndex(this string text, int index, char c)
    {
        var stringBuilder = new StringBuilder(text);
        stringBuilder[index] = c;
        return stringBuilder.ToString();
    }
    
    public static char[,] RotateBy90(char[,] grid) 
    {
        char[,] rotated = new char[grid.GetLength(1), grid.GetLength(0)];

        // rotate values
        for(int j = 0; j < grid.GetLength(1); j++) {
            for(int i = 0; i < grid.GetLength(0); i++) {
                rotated[i,j] = grid[j,i];
            }
        }

        return rotated;
    }
    
    public static long Factorial(int n)
    {
        if (n < 0) throw new ArgumentException("Input should be a non-negative integer.");

        long result = 1;
        for (int i = 2; i <= n; i++) {
            result *= i;
        }

        return result;
    }
    
    	
    // is there a better way?
    public static char[,] ToCharArray(List<string> input)
    {
        char[,] tempShape = new char[input[0].Length,input.Count];
        for(int j = 0; j < input.Count; j++) {
            for(int i = 0; i < input[j].Length; i++) {
                tempShape[i,j] = input[i][j];
            }
        }
        return tempShape;
    }
    
    
    public static long GCF(long a, long b)
    {
        while (b != 0)
        {
            long temp = b;
            b = a % b;
            a = temp;
        }
        return a;
    }

    public static long LCM(long a, long b)
    {
        return (a / GCF(a, b)) * b;
    }
}