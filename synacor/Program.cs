namespace synacor
{
    class Program
    {
        static void Main(string[] args)
        {
            var architecture = new Architecture();
            architecture.ReadProgram("challenge.bin");
            architecture.Process();
        }
    }
}
