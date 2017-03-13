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
            TreeReadModel.FreePortsToggleButton.State =
                TreeReadModel.FreePortsToggleButton.State == FreePortsDisplayMode.Hide
                    ? FreePortsDisplayMode.Show
                    : FreePortsDisplayMode.Hide;
        }

    }
}
