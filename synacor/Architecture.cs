using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace synacor
{
    public class Architecture
    {
        private readonly int[] _memory = new int[32768];
        private readonly int[] _registers = new int[8];
        private readonly Stack<int> _stack = new Stack<int>();
        private int _size;
        private string _moves = string.Empty;
        private Dictionary<int, string> _coins = new Dictionary<int, string>
        {
            {2, "red"},
            {3, "corroded"},
            {5, "shiny"},
            {7, "concave"},
            {9, "blue"}
        };

        public List<string> GetCoinCombination()
        {
            foreach (var perm in Permute(_coins.Keys.ToList()))
            {
                if (CoinCombination(perm[0], perm[1], perm[2], perm[3], perm[4]))
                {
                    return perm.Select(x => _coins[x]).ToList();
                }
            }
            throw new Exception("Invalid list of coins");
        }
        
        public void Process(string filename, bool useKnownMoves = true)
        {
            ReadProgram(filename);
            if (useKnownMoves)
            {
                _moves = File.ReadAllText("moves.txt");
            }

            int pos = 0;
            while (pos < _size)
            {
                pos = ProcessCommand(pos);
            }
        }
        private void ReadProgram(string filename)
        {
            var content = File.ReadAllBytes(filename);

            for (var i = 1; i < content.Length; i += 2)
            {
                var value = content[i - 1] + (content[i] << 8);
                if (value < 32776)
                {
                    _memory[_size++] = value;
                }
            }
        }

        private int GetRegister(int value)
        {
            return value % 32768;
        }

        private int GetRealValue(int value)
        {
            // value or register value
            if (value < 32768)
            {
                return value;
            }
            return _registers[GetRegister(value)];
        }

        private int ProcessCommand(int pos)
        {
            switch (_memory[pos])
            {
                case 0:
                    return _size;
                case 1:
                    _registers[GetRegister(_memory[pos + 1])] = GetRealValue(_memory[pos + 2]);
                    return pos + 3;
                case 2:
                    _stack.Push(GetRealValue(_memory[pos + 1]));
                    return pos + 2;
                case 3:
                    if (_stack.Count == 0)
                    {
                        throw new ArgumentException("Empty stack");
                    }
                    _registers[GetRegister(_memory[pos + 1])] = _stack.Pop();
                    return pos + 2;
                case 4:
                    _registers[GetRegister(_memory[pos + 1])] =
                        GetRealValue(_memory[pos + 2]) == GetRealValue(_memory[pos + 3]) ? 1 : 0;
                    return pos + 4;
                case 5:
                    _registers[GetRegister(_memory[pos + 1])] =
                        GetRealValue(_memory[pos + 2]) > GetRealValue(_memory[pos + 3]) ? 1 : 0;
                    return pos + 4;
                case 6:
                    return GetRealValue(_memory[pos + 1]);
                case 7:
                    if (GetRealValue(_memory[pos + 1]) != 0)
                    {
                        return GetRealValue(_memory[pos + 2]);
                    }
                    return pos + 3;
                case 8:
                    if (GetRealValue(_memory[pos + 1]) == 0)
                    {
                        return GetRealValue(_memory[pos + 2]);
                    }
                    return pos + 3;
                case 9:
                    _registers[GetRegister(_memory[pos + 1])] =
                        (GetRealValue(_memory[pos + 2]) + GetRealValue(_memory[pos + 3])) % 32768;
                    return pos + 4;
                case 10:
                    _registers[GetRegister(_memory[pos + 1])] =
                        GetRealValue(_memory[pos + 2]) * GetRealValue(_memory[pos + 3]) % 32768;
                    return pos + 4;
                case 11:
                    _registers[GetRegister(_memory[pos + 1])] =
                        GetRealValue(_memory[pos + 2]) % GetRealValue(_memory[pos + 3]);
                    return pos + 4;
                case 12:
                    _registers[GetRegister(_memory[pos + 1])] =
                        GetRealValue(_memory[pos + 2]) & GetRealValue(_memory[pos + 3]);
                    return pos + 4;
                case 13:
                    _registers[GetRegister(_memory[pos + 1])] =
                        GetRealValue(_memory[pos + 2]) | GetRealValue(_memory[pos + 3]);
                    return pos + 4;
                case 14:
                    uint tmp = (uint) GetRealValue(_memory[pos + 2]);
                    var value = (~tmp << 17) >> 17;
                    _registers[GetRegister(_memory[pos + 1])] = (int) value;
                    return pos + 3;
                case 15:
                    _registers[GetRegister(_memory[pos + 1])] = _memory[GetRealValue(_memory[pos + 2])];
                    return pos + 3;
                case 16:
                    _memory[GetRealValue(_memory[pos + 1])] = GetRealValue(_memory[pos + 2]);
                    return pos + 3;
                case 17:
                    _stack.Push(pos + 2);
                    return GetRealValue(_memory[pos + 1]);
                case 18:
                    if (_stack.Count == 0)
                    {
                        return _size;
                    }
                    return GetRealValue(_stack.Pop());
                case 19:
                    Console.Write((char)GetRealValue(_memory[pos + 1]));
                    return pos + 2;
                case 20:
                    if (_moves.Length > 0)
                    {
                        if (_moves[0] == 13)
                        {
                            _moves = _moves.Substring(1);
                        }
                        _registers[GetRegister(_memory[pos + 1])] = _moves[0];
                        Console.Write(_moves[0]);                         
                        _moves = _moves.Substring(1);
                    }
                    else
                    {
                        var key = Console.ReadKey().KeyChar;
                        _registers[GetRegister(_memory[pos + 1])] = key != 13 ? key : 10;
                    }
                    return pos + 2;
                case 21:
                    return pos + 1;
                default:
                    throw new ArgumentException("Invalid command");
            }
        }

        private static List<List<int>> Permute(List<int> list)
        {
            var result = new List<List<int>>();
            if (list.Count == 1)
            {
                result.Add(new List<int> {list[0]});
            }
            else
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var permutations = Permute(list.Take(i).Concat(list.Skip(i + 1)).ToList());
                    foreach (var perm in permutations)
                    {
                        result.Add(new List<int> { list[i] }.Concat(perm).ToList());
                    }                    
                }
            }
            return result;
        }

        private static bool CoinCombination(int a, int b, int c, int d, int e)
        {
            return a + b * c * c + d * d * d - e == 399;
        }
    }
}