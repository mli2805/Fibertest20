using System;
using System.Collections.Generic;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class MeasurementEventArgs : EventArgs
    {
        public readonly ReturnCode Code;
        public readonly Trace Trace;
        public readonly List<string> Lines;
        public readonly byte[] SorBytes;

        public MeasurementEventArgs(ReturnCode code, Trace trace, string message, byte[] sorBytes = null)
        {
            Code = code;
            Trace = trace;
            Lines = new List<string>() { message };
            SorBytes = sorBytes;
        }

        public MeasurementEventArgs(ReturnCode code, Trace trace, List<string> lines, byte[] sorBytes = null)
        {
            Code = code;
            Trace = trace;
            Lines = lines;
            SorBytes = sorBytes;
        }
    }

}