using Caliburn.Micro;

namespace Iit.Fibertest.TestBench
{
    public class LeftPanelViewModel : PropertyChangedBase
    {
        public TreeReadModel TreeReadModel { get; set; }

        public LeftPanelViewModel(TreeReadModel treeReadModel)
        {
            TreeReadModel = treeReadModel;
        }

        public void Expand()
        {
            
        }
    }

}
