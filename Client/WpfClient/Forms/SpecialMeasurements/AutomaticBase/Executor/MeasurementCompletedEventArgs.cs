using System;
using System.Collections.Generic;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class MeasurementCompletedEventArgs : EventArgs
    {
        public readonly MeasurementCompletedStatus CompletedStatus;
        public readonly Trace Trace;
        public readonly List<string> Lines;
        public readonly byte[] SorBytes;

        public MeasurementCompletedEventArgs(MeasurementCompletedStatus completedStatus, Trace trace, string message, byte[] sorBytes = null)
        {
            CompletedStatus = completedStatus;
            Trace = trace;
            Lines = new List<string>() { message };
            SorBytes = sorBytes;
        }

        public MeasurementCompletedEventArgs(MeasurementCompletedStatus completedStatus, Trace trace, List<string> lines, byte[] sorBytes = null)
        {
            CompletedStatus = completedStatus;
            Trace = trace;
            Lines = lines;
            SorBytes = sorBytes;
        }
    }
}