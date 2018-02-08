using System;
using System.Linq;
using Iit.Fibertest.Graph.Requests;
using Iit.Fibertest.StringResources;

namespace Graph.Tests
{
    public static class TraceDefiner
    {
        public static Iit.Fibertest.Graph.Trace DefineTrace(this SystemUnderTest sut, Guid lastNodeId, Guid nodeForRtuId, string title = @"some title",
            int nodeCount = 2)
        {
            sut.FakeWindowManager.RegisterHandler(model =>
                sut.OneLineMessageBoxAnswer(Resources.SID_Accept_the_path, Answer.Yes, model));
            for (int i = 0; i < nodeCount; i++)
            {
                sut.FakeWindowManager.RegisterHandler(model => sut.TraceContentChoiceHandler(model, Answer.Yes, 0));
            }

            sut.FakeWindowManager.RegisterHandler(model => sut.AddTraceViewHandler(model, title, "", Answer.Yes));
            sut.ShellVm.ComplyWithRequest(new RequestAddTrace() { LastNodeId = lastNodeId, NodeWithRtuId = nodeForRtuId });
            sut.Poller.EventSourcingTick().Wait();
            return sut.ShellVm.ReadModel.Traces.Last();
        }
    }
}