using System;
using System.Collections.Generic;

namespace synacor
{
    class Program
    {
        static void Main(string[] args)
        {
            var perm = Architecture.Permute(new List<int> {1, 2, 3, 4, 5});
            foreach (var p in perm)
            {
                Console.WriteLine(string.Join(",", p));
            }
            //var architecture = new Architecture();
            //architecture.Process("challenge.bin");
        }
    }
}
