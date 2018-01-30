using System;
using System.Linq;
using Iit.Fibertest.Client;

namespace Graph.Tests
{
    public class SutForTraceAttach : SystemUnderTest
    {
        
        public RtuLeaf TraceCreatedAndRtuInitialized(out Guid traceId, out Guid rtuId, string traceTitle = "some title")
        {
            traceId = CreateTraceRtuEmptyTerminal(traceTitle).Id;
            var id = traceId;
            rtuId = ReadModel.Traces.First(t => t.Id == id).RtuId;
            return this.InitializeRtu(rtuId, @"192.168.96.58");
        }

       
    }
}