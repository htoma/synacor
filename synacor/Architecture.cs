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
                    var key = Console.ReadKey();
                    _registers[GetRegister(_memory[pos + 1])] = key.KeyChar != 13 ? key.KeyChar : 10;
                    
                    return pos + 2;
                case 21:
                    return pos + 1;
                default:
                    throw new ArgumentException("Invalid command");
            }
        }
    }
}