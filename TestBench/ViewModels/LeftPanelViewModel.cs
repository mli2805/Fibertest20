using Caliburn.Micro;

namespace Iit.Fibertest.TestBench
{
    public class LeftPanelViewModel : PropertyChangedBase
    {
        public TreeReadModel TreeReadModel { get; set; }
        public int RtuCount => TreeReadModel.Tree.Count;
        public int FullPortCount => TreeReadModel.Tree.PortCount();

        public LeftPanelViewModel(TreeReadModel treeReadModel)
        {
            TreeReadModel = treeReadModel;
        }


    }

}
