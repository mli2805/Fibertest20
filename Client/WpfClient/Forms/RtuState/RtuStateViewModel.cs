using Caliburn.Micro;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class RtuStateViewModel : Screen
    {
        public RtuStateVm Model { get; set; }

        public void Initialize(RtuStateVm model)
        {
            Model = model;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_State_of_RTU;
        }

        public void Close()
        {
            TryClose();
        }
    }
}
