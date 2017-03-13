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
            TreeOfRtuModel.FreePortsVisibility.State =
                TreeOfRtuModel.FreePortsVisibility.State == FreePortsVisibilityState.Hide
                    ? FreePortsVisibilityState.Show
                    : FreePortsVisibilityState.Hide;
        }

    }
}
