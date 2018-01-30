using Iit.Fibertest.Client;

namespace Graph.Tests
{
    public class SutForBaseRefs : SystemUnderTest
    {
        public bool BaseRefAssignHandler2(object model, string precisePath, string fastPath, string aditionalPath, Answer answer)
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