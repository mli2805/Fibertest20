using Caliburn.Micro;

namespace Iit.Fibertest.TestBench
{
    public class TreeOfRtuViewModel : PropertyChangedBase
    {
        public TreeOfRtuModel TreeOfRtuModel { get; set; }

        public TreeOfRtuViewModel(TreeOfRtuModel treeOfRtuModel)
        {
            TreeOfRtuModel = treeOfRtuModel;
        }

        public void ChangeFreePortsVisibility()
        {
            TreeOfRtuModel.FreePorts.AreVisible = !TreeOfRtuModel.FreePorts.AreVisible;
        }

    }
}
