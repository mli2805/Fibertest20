using Caliburn.Micro;

namespace Iit.Fibertest.Client
{
    public class InputTraceTableViewModel : Screen
    {
        private RtuLeaf _rtuLeaf;

        protected override void OnViewLoaded(object view)
        {
            DisplayName = $@"Input trace  (RTU:  {_rtuLeaf.Title})";
        }

        public void Initialize(RtuLeaf rtuLeaf)
        {
            _rtuLeaf = rtuLeaf;
        }

    }
}
