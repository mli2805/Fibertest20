using RtuManagement;


namespace ConsoleAppOtdr
{
    class Program
    {
        static void Main()
        {
            var rtuManager = new RtuManager();
            rtuManager.Start();
        }

    }
}