namespace AoC2024;

public class Day4
{
    private char[,] _grid;
    
    public void Run(List<string> input)
    {
        _grid = Utils.ToCharArray(input);

        int part1 = 0;
        int part2 = 0;
        for (int i = 0; i < _grid.GetLength(0); i++) {
            for (int j = 0; j < _grid.GetLength(1); j++) {
                if (_grid[i, j] == 'X') part1 += TryFindXmas(i, j);
                if (_grid[i, j] == 'A') part2 += TryFindMasX(i, j);
            }
        }
        Console.WriteLine($"Part 1: {part1}");
        Console.WriteLine($"Part 2: {part2}");
    }

    private int TryFindXmas(int x, int y)
    {
        int count = 0;
        // all directions
        for (int i = -1; i <= 1; i++) {
            for (int j = -1; j <= 1; j++) {
                if (_grid.TryGetValue(x+i, y+j) == 'M' && _grid.TryGetValue(x+i+i, y+j+j) == 'A' && _grid.TryGetValue(x+i+i+i, y+j+j+j) == 'S') {
                    count++;
                }
            }
        }
        return count;
    }
    
    private int TryFindMasX(int x, int y)
    {
        int masCount = 0;
        // all diagonal directions
        for (int i = -1; i <= 1; i++) {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 || j == 0) continue;
                if (_grid.TryGetValue(x + i, y + j) == 'M' && _grid.TryGetValue(x - i, y - j) == 'S') masCount++;
            }
        }
        return (masCount == 2) ? 1 : 0;
    }
}