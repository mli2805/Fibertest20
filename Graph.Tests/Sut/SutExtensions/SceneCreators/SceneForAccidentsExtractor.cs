using Iit.Fibertest.Client;

namespace Graph.Tests
{
    public static class SceneForAccidentsExtractor
    {
        public static Iit.Fibertest.Graph.Trace SetTraceWithBaseRefs(this SystemUnderTest sut)
        {
            var rtu = sut.SetInitializedRtu();
            var trace = sut.SetTrace(rtu.NodeId, @"Trace1");
            var traceLeaf = (TraceLeaf)sut.TreeOfRtuModel.Tree.GetById(trace.Id);
            sut.AssignBaseRef(traceLeaf, SystemUnderTest.Base1550Lm4RealplaceYesRough, SystemUnderTest.Base1550Lm4RealplaceYesRough, null, Answer.Yes);
            return trace;
        }


    }
}