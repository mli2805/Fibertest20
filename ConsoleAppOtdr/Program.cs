using System;
using Iit.Fibertest.Utils35;


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