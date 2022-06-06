using System;
using System.Collections.Generic;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class MeasurementCompletedEventArgs : EventArgs
    {
        public MeasurementCompletedStatus CompletedStatus;
        public List<string> Lines;
        public byte[] SorBytes;

        // debug
        public Trace Trace;
        public Guid ModelGuid;
        public Guid ExecutorGuid;

        public MeasurementCompletedEventArgs(MeasurementCompletedStatus completedStatus, string message, byte[] sorBytes = null)
        {
            CompletedStatus = completedStatus;
            Lines = new List<string>() {message};
            SorBytes = sorBytes;
        }
        
        public MeasurementCompletedEventArgs(MeasurementCompletedStatus completedStatus, List<string> lines, byte[] sorBytes = null)
        {
            CompletedStatus = completedStatus;
            Lines = lines;
            SorBytes = sorBytes;
        }
    }
}