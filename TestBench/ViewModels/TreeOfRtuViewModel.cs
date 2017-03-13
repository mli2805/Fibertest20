using Caliburn.Micro;

namespace Iit.Fibertest.TestBench
{
    public class TreeOfRtuViewModel : PropertyChangedBase
    {
        public TreeReadModel TreeReadModel { get; set; }

        public TreeOfRtuViewModel(TreeReadModel treeReadModel)
        {
            TreeReadModel = treeReadModel;
        }

        public void ChangeFreePortsVisibility()
        {
            TreeReadModel.FreePortsVisibility.State =
                TreeReadModel.FreePortsVisibility.State == FreePortsVisibilityState.Hide
                    ? FreePortsVisibilityState.Show
                    : FreePortsVisibilityState.Hide;
        }

    }
}
