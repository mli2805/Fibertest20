using System;
using System.Linq;
using Iit.Fibertest.Client;

namespace Graph.Tests
{
    public static class SceneForTraceAttach
    {
        public static RtuLeaf TraceCreatedAndRtuInitialized(this SystemUnderTest sut, out Guid traceId, out Guid rtuId, string traceTitle = "some title")
        {
            traceId = sut.CreateTraceRtuEmptyTerminal(traceTitle).Id;
            var id = traceId;
            rtuId = sut.ReadModel.Traces.First(t => t.Id == id).RtuId;
            return sut.InitializeRtu(rtuId, @"192.168.96.58");
        }
    }
}