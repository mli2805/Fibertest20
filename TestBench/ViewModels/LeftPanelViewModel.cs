using Caliburn.Micro;

namespace Iit.Fibertest.TestBench
{
    public class LeftPanelViewModel : PropertyChangedBase
    {
        public TreeReadModel TreeReadModel { get; set; }
        public int RtuCount => TreeReadModel.Tree.Count;
        public int FullPortCount => TreeReadModel.Tree.PortCount();
        public int FullTraceCount => TreeReadModel.Tree.TraceCount();
        public string Occupancy => $@"{(double)FullTraceCount / FullPortCount * 100:F}%";

        public LeftPanelViewModel(TreeReadModel treeReadModel)
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
