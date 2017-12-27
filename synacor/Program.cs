namespace synacor
{
    class Program
    {
        static void Main(string[] args)
        {
            var architecture = new Architecture();
            architecture.Process("challenge.bin", true, "log.txt");
        }
    }
}
