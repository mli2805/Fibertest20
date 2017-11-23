using System.Windows.Media;
using Caliburn.Micro;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public class TraceStateViewModel : Screen
    {

        public TraceStateVm Model { get; set; }
        public Measurement Measurement { get; set; }

        public TraceStateViewModel()
        {

        }

        public void Initialize(TraceStateVm model)
        {
            Model = model;
        }
        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Trace state";
        }
    }
}
