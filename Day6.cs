namespace AoC2024;

public class Day6
{
    private char[,] _inputGrid;
    private char[,] _grid;
    private Vector2Int[,] _visitedDirs;
    private Vector2Int[] _directions = {new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(0,-1)};

    public void Run(List<string> input)
    {
        _inputGrid = Utils.ToCharArray(input);
        
        _grid = _inputGrid.Clone() as char[,];
        Console.WriteLine($"Part 1: {SimulateGuard()}");        

        // part 2: brute force - try block in every position, simulate guard and find loop
        int blocks = _grid.Cast<char>().Where((c, index) => TryBlock(index)).Count();
        Console.WriteLine($"Part 2: {blocks}");        
    }

    private int SimulateGuard()
    {
        // find guard 
        _visitedDirs = new Vector2Int[_grid.GetLength(0), _grid.GetLength(1)];
        Vector2Int index = new Vector2Int();
        for (int i = 0; i < _grid.GetLength(0); i++)
            for (int j = 0; j < _grid.GetLength(1); j++)
                if (_grid[i, j] == '^')
                    index = new Vector2Int(i, j);
        _grid[index.x, index.y] = 'X';
        _visitedDirs[index.x, index.y] = _directions[0];
        
        int visited = 1;
        int directionIndex = 0;
        bool walking = true;
        while (walking)
        {
            char nextPos = _grid.TryGetValue(index.x + _directions[directionIndex].x, index.y + _directions[directionIndex].y);
            if (nextPos == '\0') {
                walking = false;
            }
            else
            {
                if (nextPos == '.' || nextPos == 'X') {
                    if (nextPos == '.') visited++;
                    index = new Vector2Int(index.x + _directions[directionIndex].x, index.y + _directions[directionIndex].y);
                    if (_visitedDirs[index.x, index.y] == _directions[directionIndex])
                    {
                        // Console.WriteLine($"LOOP");
                        return 0;
                    }
                    // Console.WriteLine($"{index} {directionIndex}");
                    _grid[index.x, index.y] = 'X';
                    _visitedDirs[index.x, index.y] = _directions[directionIndex];
                }
                else {
                    directionIndex = (directionIndex + 1) % 4; // change direction
                    // Console.WriteLine($"turn {directionIndex}");
                }
            }
        }
        return visited;
    }

    private bool TryBlock(int rawIndex)
    {
        Vector2Int index = new Vector2Int(rawIndex/_grid.GetLength(1), rawIndex%_grid.GetLength(0));
        // reset grid and put block
        _grid = (_inputGrid.Clone() as char[,])!;
        _grid[index.x, index.y] = '0';
        
        return SimulateGuard() == 0;
    }
}