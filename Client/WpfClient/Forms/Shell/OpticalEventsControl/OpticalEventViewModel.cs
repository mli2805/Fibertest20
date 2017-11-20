using Caliburn.Micro;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public class OpticalEventViewModel : Screen
    {
        public OpticalEventVm OpticalEventVm { get; set; }

        public OpticalEventViewModel(OpticalEventVm opticalEventVm)
        {
            OpticalEventVm = opticalEventVm;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = @"Optical event";
        }

        public void Save()
        {
            
        }
    }
}
