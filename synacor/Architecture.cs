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
        private readonly Dictionary<int, string> _coins = new Dictionary<int, string>
        {
            {2, "red"},
            {3, "corroded"},
            {5, "shiny"},
            {7, "concave"},
            {9, "blue"}
        };

        private string _logFilename;
        private bool _log;

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
        
        public void Process(string filename, bool useKnownMoves, string logFilename)
        {
            ReadProgram(filename);
            if (useKnownMoves)
            {
                _moves = File.ReadAllText("moves.txt");
            }

            _logFilename = logFilename;

            int pos = 0;
            while (pos < _size)
            {
                pos = ProcessCommand(pos);
            }
        }

        private void Log(int pos, string line)
        {
            if (!_log && pos == 6027)
            {
                _log = true;
            }
            if (line == "call (17): 6027")
            {
                //File.AppendAllLines(_logFilename, new[] { $"r0: {_registers[0]}, r1: {_registers[1]}" });
            }
            if (_log)
            {
                //File.AppendAllLines(_logFilename, new[] {$"{pos}: {line}"});
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
                    Log(pos, $"set   (1): {_memory[pos + 1]} {_memory[pos + 2]}");
                    _registers[GetRegister(_memory[pos + 1])] = GetRealValue(_memory[pos + 2]);
                    return pos + 3;
                case 2:
                    Log(pos, $"push  (2): {_memory[pos + 1]}");
                    _stack.Push(GetRealValue(_memory[pos + 1]));
                    return pos + 2;
                case 3:
                    Log(pos, $"pop   (3): {_memory[pos + 1]}");
                    if (_stack.Count == 0)
                    {
                        throw new ArgumentException("Empty stack");
                    }
                    _registers[GetRegister(_memory[pos + 1])] = _stack.Pop();
                    return pos + 2;
                case 4:
                    Log(pos, $"eq    (4): {_memory[pos + 1]} {_memory[pos + 2]} {_memory[pos + 3]}");
                    _registers[GetRegister(_memory[pos + 1])] =
                        GetRealValue(_memory[pos + 2]) == GetRealValue(_memory[pos + 3]) ? 1 : 0;
                    return pos + 4;
                case 5:
                    Log(pos, $"gq    (5): {_memory[pos + 1]} {_memory[pos + 2]} {_memory[pos + 3]}");
                    _registers[GetRegister(_memory[pos + 1])] =
                        GetRealValue(_memory[pos + 2]) > GetRealValue(_memory[pos + 3]) ? 1 : 0;
                    return pos + 4;
                case 6:
                    Log(pos, $"jmp   (6): {_memory[pos + 1]}");
                    return GetRealValue(_memory[pos + 1]);
                case 7:
                    Log(pos, $"jt    (7): {_memory[pos + 1]} {_memory[pos + 2]}");
                    if (GetRealValue(_memory[pos + 1]) != 0)
                    {
                        return GetRealValue(_memory[pos + 2]);
                    }
                    return pos + 3;
                case 8:
                    Log(pos, $"jf    (8): {_memory[pos + 1]} {_memory[pos + 2]}");
                    if (GetRealValue(_memory[pos + 1]) == 0 /*|| _memory[pos + 1] == 32775*/)
                    {
                        return GetRealValue(_memory[pos + 2]);
                    }
                    return pos + 3;
                case 9:
                    Log(pos, $"add   (9): {_memory[pos + 1]} {_memory[pos + 2]} {_memory[pos + 3]}");
                    _registers[GetRegister(_memory[pos + 1])] =
                        (GetRealValue(_memory[pos + 2]) + GetRealValue(_memory[pos + 3])) % 32768;
                    return pos + 4;
                case 10:
                    Log(pos, $"mult (10): {_memory[pos + 1]} {_memory[pos + 2]} {_memory[pos + 3]}");
                    _registers[GetRegister(_memory[pos + 1])] =
                        GetRealValue(_memory[pos + 2]) * GetRealValue(_memory[pos + 3]) % 32768;
                    return pos + 4;
                case 11:
                    Log(pos, $"mod  (11): {_memory[pos + 1]} {_memory[pos + 2]} {_memory[pos + 3]}");
                    _registers[GetRegister(_memory[pos + 1])] =
                        GetRealValue(_memory[pos + 2]) % GetRealValue(_memory[pos + 3]);
                    return pos + 4;
                case 12:
                    Log(pos, $"and  (12): {_memory[pos + 1]} {_memory[pos + 2]} {_memory[pos + 3]}");
                    _registers[GetRegister(_memory[pos + 1])] =
                        GetRealValue(_memory[pos + 2]) & GetRealValue(_memory[pos + 3]);
                    return pos + 4;
                case 13:
                    Log(pos, $"or   (13): {_memory[pos + 1]} {_memory[pos + 2]} {_memory[pos + 3]}");
                    _registers[GetRegister(_memory[pos + 1])] =
                        GetRealValue(_memory[pos + 2]) | GetRealValue(_memory[pos + 3]);
                    return pos + 4;
                case 14:
                    Log(pos, $"not  (14): {_memory[pos + 1]} {_memory[pos + 2]} {_memory[pos + 3]}");
                    uint tmp = (uint) GetRealValue(_memory[pos + 2]);
                    var value = (~tmp << 17) >> 17;
                    _registers[GetRegister(_memory[pos + 1])] = (int) value;
                    return pos + 3;
                case 15:
                    Log(pos, $"rmem (15): {_memory[pos + 1]} {_memory[pos + 2]}");
                    _registers[GetRegister(_memory[pos + 1])] = _memory[GetRealValue(_memory[pos + 2])];
                    return pos + 3;
                case 16:
                    Log(pos, $"wmem (16): {_memory[pos + 1]} {_memory[pos + 2]}");
                    _memory[GetRealValue(_memory[pos + 1])] = GetRealValue(_memory[pos + 2]);
                    return pos + 3;
                case 17:
                    Log(pos, $"call (17): {_memory[pos + 1]}");
                    if (_memory[pos + 1] == 6027)
                    {
                        _registers[0] = 6;
                        _registers[7] = 25734;
                        return pos + 2;
                    }
                    _stack.Push(pos + 2);
                    return GetRealValue(_memory[pos + 1]);
                case 18:
                    Log(pos, $"ret  (18):");
                    if (_stack.Count == 0)
                    {
                        return _size;
                    }
                    return GetRealValue(_stack.Pop());
                case 19:
                    Log(pos, $"out  (19): {_memory[pos + 1]}");
                    Console.Write((char)GetRealValue(_memory[pos + 1]));
                    return pos + 2;
                case 20:
                    Log(pos, $"in   (20): {_memory[pos + 1]}");
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
                        //dummy value for detecting teleporter algorithm
                        _registers[7] = 1;
                        _log = true;
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