namespace AoC2024;

public class Day6
{
    private char[,] _inputGrid;
    private char[,] _walkedGrid;
    private Vector2Int startPos;
    
    private static Vector2Int[] _directions = {new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(0,-1)};

    public void Run(List<string> input)
    {
        _inputGrid = Utils.ToCharArray(input);
        startPos = FindGuard();
        
        char[,] grid = (char[,])_inputGrid.Clone();
        Console.WriteLine($"Part 1: {SimulateGuard(grid)}");        
        
        _walkedGrid = (char[,])grid.Clone();
        
        // part 2: brute force - try block in (almost) every position, simulate guard and find loop
        // single-threaded:
        // int blocks = _inputGrid.Cast<char>().Where((c, index) => TryBlock(index)).Count();
        // Console.WriteLine($"Part 2: {blocks}");
        
        // multi-threaded:
        int counter = 0;
        Parallel.ForEach(_inputGrid.Cast<char>(), (c, state, index) => {
            if (TryBlock((int) index)) counter++;   
        });
        Console.WriteLine($"Part 2: {counter}");
    }

    private Vector2Int FindGuard()
    {
        for (int i = 0; i < _inputGrid.GetLength(0); i++)
            for (int j = 0; j < _inputGrid.GetLength(1); j++)
                if (_inputGrid[i, j] == '^')
                    return new Vector2Int(i, j);
        throw new Exception("Guard not found");
    }

    private int SimulateGuard(char[,] grid)
    {
        Vector2Int[,] visitedDirs = new Vector2Int[grid.GetLength(0), grid.GetLength(1)];

        Vector2Int index = startPos;
        grid[index.x, index.y] = 'X';
        visitedDirs[index.x, index.y] = _directions[0];
        
        int visited = 1;
        int directionIndex = 0;
        bool walking = true;
        while (walking)
        {
            char nextPos = grid.TryGetValue(index.x + _directions[directionIndex].x, index.y + _directions[directionIndex].y);
            if (nextPos == '\0') {
                walking = false; // out of bounds, stop
            }
            else
            {
                if (nextPos == '.' || nextPos == 'X') {
                    if (nextPos == '.') visited++;
                    index = new Vector2Int(index.x + _directions[directionIndex].x, index.y + _directions[directionIndex].y);
                    if (visitedDirs[index.x, index.y] == _directions[directionIndex]) {
                        return 0;
                    }
                    grid[index.x, index.y] = 'X';
                    visitedDirs[index.x, index.y] = _directions[directionIndex];
                }
                else {
                    directionIndex = (directionIndex + 1) % 4; // change direction
                }
            }
        }
        return visited;
    }

    private bool TryBlock(int rawIndex)
    {
        Vector2Int index = new Vector2Int(rawIndex/_inputGrid.GetLength(1), rawIndex%_inputGrid.GetLength(0));
        if (_inputGrid[index.x, index.y] != '.') return false; // skip non-free positions
        if (_walkedGrid[index.x, index.y] == '.') return false; // skip positions not come across in the normal run 
        
        // reset grid and put block
        char[,] grid = (_inputGrid.Clone() as char[,])!;
        grid[index.x, index.y] = '0';
        
        return SimulateGuard(grid) == 0;
    }
}