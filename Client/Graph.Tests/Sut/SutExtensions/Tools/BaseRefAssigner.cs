using System;
using System.Linq;
using Iit.Fibertest.Client;
using Iit.Fibertest.StringResources;

namespace Graph.Tests
{
    public static class BaseRefAssigner
    {
        public static void AssignBaseRef(this SystemUnderTest sut, TraceLeaf traceLeaf,
            string precisePath, string fastPath, string aditionalPath, Answer answer)
        {
            if (!String.IsNullOrEmpty(precisePath))
                sut.FakeWindowManager.RegisterHandler(model => sut.ManyLinesMessageBoxAnswer(Answer.Yes, model)); // about length
            sut.FakeWindowManager.RegisterHandler(model => sut.ManyLinesMessageBoxAnswer(Answer.Yes, model)); // about wrong base

            sut.FakeWindowManager.RegisterHandler(model => BaseRefAssignHandler2(model, precisePath, fastPath, aditionalPath, answer));
            traceLeaf.MyContextMenu.First(i => i.Header == Resources.SID_Base_refs_assignment).Command.Execute(traceLeaf);
            sut.Poller.EventSourcingTick().Wait();
        }

        private static bool BaseRefAssignHandler2(object model, string precisePath, string fastPath, string aditionalPath, Answer answer)
        {
            if (!(model is BaseRefsAssignViewModel vm)) return false;
            if (answer == Answer.Yes)
            {
                if (precisePath == "")
                    vm.ClearPathToPrecise();
                else if (precisePath != null)
                    vm.PreciseBaseFilename = precisePath;

                if (fastPath == "")
                    vm.ClearPathToFast();
                else if (fastPath != null)
                    vm.FastBaseFilename = fastPath;

                if (aditionalPath == "")
                    vm.ClearPathToAdditional();
                else if (aditionalPath != null)
                    vm.AdditionalBaseFilename = aditionalPath;

                vm.Save().Wait();
            }
            else
                vm.Cancel();
            return true;
        }
    }
}