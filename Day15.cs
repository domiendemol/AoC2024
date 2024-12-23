using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text.RegularExpressions;

namespace AoC2024;

public class Day15
{
    private char[,] _grid;
    private List<char> _moves;
    private Vector2Int _robotPos;
    private Dictionary<char, Vector2Int> _dirMap = new Dictionary<char, Vector2Int>        {
        { '>', new Vector2Int(0,1) },
        { '<', new Vector2Int(0,-1) },
        { '^', new Vector2Int(-1,0) },
        { 'v', new Vector2Int(1,0) },
    };
    
    public void Run(List<string> input)
    {
        // Part 1
        ParseInput(input, false);
        Enumerable.Range(0, _moves.Count).ToList().ForEach(i => Move(i));
        Console.WriteLine($"Part 1: {Enumerable.Range(0,_grid.GetLength(0)*_grid.GetLength(1)).Sum(i => GetGPS(i))}");
        
        // Part 2
        ParseInput(input, true);
        for (int i = 0; i < _moves.Count; i++) {
            MoveDoubleBoxes(i);
            // Console.Clear();
            // Utils.PrintCharArray(_grid);
            // Console.WriteLine($"{i}, next: {_moves.TryGetValue(i+1)}");
            // Console.ReadLine();
        }
        Console.WriteLine($"Part 2: {Enumerable.Range(0,_grid.GetLength(0)*_grid.GetLength(1)).Sum(i => GetGPS(i))}");
        // Utils.PrintCharArray(_grid);
    }

    private void ParseInput(List<string> input, bool widen)
    {
        // parse map
        _grid = Utils.ToCharArray(input.Where(line => line.StartsWith("#")).ToList());
        if (widen)
        {
            char[,] doubleGrid = new char[_grid.GetLength(0), _grid.GetLength(1)*2];
            for (int i = 0; i < _grid.GetLength(0); i++)
            {
                for (int j = 0; j < _grid.GetLength(1); j++)
                {
                    if (_grid[i, j] == 'O') {
                        doubleGrid[i, j * 2] = '[';
                        doubleGrid[i, j * 2 + 1] = ']';
                    }
                    else if (_grid[i, j] == '@') {
                        doubleGrid[i, j * 2] = '@';
                        doubleGrid[i, j * 2 + 1] = '.';
                    }
                    else {
                        doubleGrid[i, j * 2] = _grid[i, j];
                        doubleGrid[i, j * 2 + 1] = _grid[i, j];
                    }
                }
            }
            _grid = doubleGrid;
        }
        // parse movements
        _moves = input.Where(line => !line.StartsWith("#") && line.Length > 0).SelectMany(c => c).ToList();
        // find robot
        _robotPos = Utils.FindIndex(_grid, '@');
    }

    private void Move(int mIndex)
    {
        Vector2Int dir = _dirMap[_moves[mIndex]];
        Vector2Int look = _robotPos;
        while (_grid[(look+dir).x, (look+dir).y] == 'O' || _grid[(look+dir).x, (look+dir).y] == '[' || _grid[(look+dir).x, (look+dir).y] == ']')
        {
            look += dir; // find blocks to move as well
        }
        if (_grid[(look + dir).x, (look + dir).y] == '#') return; // next position not free, not moving

        for (int i = (int)(look-_robotPos).Magnitude()+1; i > 0 ; i--)
        {
            _grid[(_robotPos+i*dir).x, (_robotPos+i*dir).y] = _grid[(_robotPos+(i-1)*dir).x, (_robotPos+(i-1)*dir).y];
        }

        _grid[_robotPos.x, _robotPos.y] = '.';
        _robotPos += dir;
    }
    
    private void MoveDoubleBoxes(int mIndex)
    {
        Vector2Int dir = _dirMap[_moves[mIndex]];
        if (_grid[(_robotPos + dir).x, (_robotPos + dir).y] == '#') return;
        if (dir.x == 0) {
            Move(mIndex);   
            return;
        }
        
        // find all blocks that would be pushed
        List<Vector2Int>? pushedBlocks = GetBlocks(_robotPos, dir);
        if (pushedBlocks != null)
        {
            // none should have a wall in front of them
            if (pushedBlocks.Any(bl => _grid[(bl+dir).x, (bl+dir).y] == '#')) return;
            // move 'em all (in reverse direction)
            pushedBlocks = pushedBlocks.Distinct().OrderBy(bl => bl.x).ToList();
            if (dir.x == 1) 
                pushedBlocks.Reverse();
            pushedBlocks.ForEach(block =>
            {
                _grid[(block+dir).x, (block+dir).y] = _grid[(block).x, (block).y]; // new place
                _grid[(block).x, (block).y] = '.';
            });
        }
        
        _grid[(_robotPos + dir).x, (_robotPos+dir).y] = '@';
        _grid[_robotPos.x, _robotPos.y] = '.';
        _robotPos += dir;
    }

    private List<Vector2Int>? GetBlocks(Vector2Int coord, Vector2Int dir)
    {
        List<Vector2Int>? tBlocks = null;
        // check two positions higher
        if (_grid[(coord+dir).x, (coord+dir).y] == '[' || _grid[(coord+dir).x, (coord+dir).y] == ']')
        {
            tBlocks = new List<Vector2Int>();
            tBlocks.Add(coord + dir);
            tBlocks.Add(coord + dir + new Vector2Int(0, _grid[(coord+dir).x, (coord+dir).y] == '[' ? 1 : -1));
            IEnumerable<Vector2Int>? a = GetBlocks(coord + dir, dir);
            IEnumerable<Vector2Int>? b = GetBlocks(coord + dir + new Vector2Int(0, _grid[(coord+dir).x, (coord+dir).y] == '[' ? 1 : -1), dir);
            if (a != null) tBlocks.AddRange(a);
            if (b != null) tBlocks.AddRange(b);
        }
        return tBlocks;
    }
    
    private int GetGPS(int index)
    {
        // The GPS coordinate of a box is equal to 100 times its distance from the top edge of the map
        // plus its distance from the left edge of the map.
        // (This process does not stop at wall tiles; measure all the way to the edges of the map.)
        int x = index % _grid.GetLength(0);
        int y = index / _grid.GetLength(0);
        return (_grid[x,y] == 'O' || _grid[x,y] == '[') ? 100 * x + y : 0;
    }
}