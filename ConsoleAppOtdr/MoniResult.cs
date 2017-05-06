using System;
using Iit.Fibertest.Utils35;

namespace ConsoleAppOtdr
{
    [Serializable]
    public class MoniResult
    {
        public int Port { get; set; }
        public DateTime TimeStamp { get; set; }
        public BaseRefType BaseRefType { get; set; }
        public FiberState TraceState { get; set; }


    }
}