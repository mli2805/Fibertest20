using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class AutoBaseViewModel : Screen
    {
        public RtuLeaf RtuLeaf { get; set; }
        private Rtu _rtu;
        
        public bool IsAnswerPositive { get; set; }

        public OtdrParametersViewModel OtdrParametersViewModel { get; set; } = new OtdrParametersViewModel();

        public void Initialize(TraceLeaf traceLeaf, Model readModel)
        {
            var parent = traceLeaf.Parent;
            RtuLeaf = parent is RtuLeaf leaf ? leaf : (RtuLeaf)parent.Parent;
            // var otauLeaf = (IPortOwner)parent;
            _rtu = readModel.Rtus.First(r => r.Id == RtuLeaf.Id);

            OtdrParametersViewModel.Initialize(_rtu.AcceptableMeasParams);
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Measurement_parameters;
        }
        public void Measure()
        {
            IsAnswerPositive = true;
            TryClose();
        }

        public void Close()
        {
            IsAnswerPositive = false;
            TryClose();
        }
    }
}
