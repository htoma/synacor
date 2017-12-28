using System;
using System.Linq;

namespace synacor
{
    public struct Pos
    {
        public int i;
        public int j;
    }

    public class Grid
    {
        //+: -1
        //-: -2
        //*: -3

        public void SolveGrid()
        {
            var grid = new int[4][];
            grid[0] = new[] {-3, 8, -2, 1};
            grid[1] = new[] {4, -3, 11, -3};
            grid[2] = new[] {-1, 4, -2, 18};
            grid[3] = new[] {22, -2, 9, -3};

            var n = 4;
            var pos = new Pos
            {
                i = 3,
                j = 0
            };
            var found = GetAround(pos, n).Any(x => FindPath(grid, n, x, -4, grid[pos.i][pos.j], 1,
                BuildPath("", grid[pos.i][pos.j])));
        }

        private string BuildPath(string cur, int val)
        {
            switch (val)
            {
                case -1:
                    return cur + " + ";
                case -2:
                    return cur + " - ";
                case -3:
                    return cur + " * ";
                default:
                    return $"{cur} {val}";
            }
        }

        private bool FindPath(int[][] grid, int n, Pos pos, int op, int sum, int length, string path)
        {
            if (grid[pos.i][pos.j] < 0)
            {
                return GetAround(pos, n).Any(x => FindPath(grid, n, x, grid[pos.i][pos.j], sum, length + 1,
                    BuildPath(path, grid[pos.i][pos.j])));
            }
            if (length > 13)
            {
                return false;
            }
            switch (op)
            {
                case -1:
                    sum += grid[pos.i][pos.j];
                    break;
                case -2:
                    sum -= grid[pos.i][pos.j];
                    break;
                case -3:
                    sum *= grid[pos.i][pos.j];
                    break;
                default:
                    throw new Exception($"Invalid op {op}");
            }

            if (sum < 0)
            {
                return false;
            }

            if (pos.i == 3 && pos.j == 0)
            {
                //can't get back to orb place
                return false;
            }

            if (pos.i == 0 && pos.j == n - 1)
            {
                if (sum == 30)
                {
                    Console.WriteLine($"Found solution: {BuildPath(path, grid[pos.i][pos.j])}");
                    return true;
                }
                return false;
            }
           
            return GetAround(pos, n).Any(x => FindPath(grid, n, x, op, sum, length + 1,
                BuildPath(path, grid[pos.i][pos.j])));
        }

        private static Pos[] GetAround(Pos pos, int n)
        {
            var result = new[]
            {
                new Pos
                {
                    i = pos.i - 1,
                    j = pos.j
                },
                new Pos
                {
                    i = pos.i + 1,
                    j = pos.j
                }
                ,
                new Pos
                {
                    i = pos.i,
                    j = pos.j - 1
                },
                new Pos
                {
                    i = pos.i,
                    j = pos.j + 1
                }
            };
            return result.Where(x => x.i >= 0 && x.i < n && x.j >= 0 && x.j < n).ToArray();
        }
    }
}