using System;
using System.Collections.Generic;
using System.IO;

namespace synacor
{
    public class Architecture
    {
        private readonly int[] _memory = new int[32768];
        private readonly int[] _registers = new int[8];
        private readonly Stack<int> _stack = new Stack<int>();
        private int _size;
        private string _terminal = string.Empty;
        
        public void ReadProgram(string filename)
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

        public void Process()
        {
            int pos = 0;
            int opCount = 0;
            while (pos < _size)
            {
                pos = ProcessCommand(pos);
                opCount++;
            }
        }

        private int GetRealValue(int value)
        {
            // value or register value
            if (value < 32768)
            {
                return value;
            }
            return _registers[value % 32768];
        }

        private int ProcessCommand(int pos)
        {
            switch (_memory[pos])
            {
                case 0:
                    return _size;
                case 1:
                    _registers[GetRealValue(_memory[pos + 1])] = GetRealValue(_memory[pos + 2]);
                    return pos + 3;
                case 2:
                    _stack.Push(GetRealValue(_memory[pos + 1]));
                    return pos + 2;
                case 3:
                    if (_stack.Count == 0)
                    {
                        throw new ArgumentException("Empty stack");
                    }
                    _registers[GetRealValue(_memory[pos + 1])] = _stack.Pop();
                    return pos + 2;
                case 4:
                    _registers[GetRealValue(_memory[pos + 1])] =
                        GetRealValue(_memory[pos + 2]) == GetRealValue(_memory[pos + 3]) ? 1 : 0;
                    return pos + 4;
                case 5:
                    _registers[GetRealValue(_memory[pos + 1])] =
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
                    _registers[GetRealValue(_memory[pos + 1])] =
                        (GetRealValue(_memory[pos + 2]) + GetRealValue(_memory[pos + 3])) % 32768;
                    return pos + 4;
                case 10:
                    _registers[GetRealValue(_memory[pos + 1])] =
                        (GetRealValue(_memory[pos + 2]) * GetRealValue(_memory[pos + 3])) % 32768;
                    return pos + 4;
                case 11:
                    _registers[GetRealValue(_memory[pos + 1])] =
                        GetRealValue(_memory[pos + 2]) % GetRealValue(_memory[pos + 3]);
                    return pos + 4;
                case 12:
                    _registers[GetRealValue(_memory[pos + 1])] =
                        GetRealValue(_memory[pos + 2]) & GetRealValue(_memory[pos + 3]);
                    return pos + 4;
                case 13:
                    _registers[GetRealValue(_memory[pos + 1])] =
                        GetRealValue(_memory[pos + 2]) | GetRealValue(_memory[pos + 3]);
                    return pos + 4;
                case 14:
                    int value = (1 << 16 - 1) & ~GetRealValue(_memory[pos + 2]);
                    _registers[GetRealValue(_memory[pos + 1])] = value;
                    return pos + 3;
                case 15:
                    _registers[GetRealValue(_memory[pos + 1])] = _memory[GetRealValue(_memory[pos + 2])];
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
                    _terminal += (char)GetRealValue(_memory[pos + 1]);
                    Console.Write((char)GetRealValue(_memory[pos + 1]));
                    return pos + 2;
                case 20:
                    Console.WriteLine(_terminal);
                    var posNewline = _terminal.IndexOf((char)10);
                    if (posNewline == -1)
                    {
                        _terminal = string.Empty;
                    }
                    else
                    {
                        _terminal = _terminal.Substring(posNewline + 1);
                    }
                    return pos + 2;
                case 21:
                    return pos + 1;
                default:
                    throw new ArgumentException("Invalid command");
            }
        }
    }
}