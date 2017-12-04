using Caliburn.Micro;

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
            DisplayName = $"State of RTU \"{Model.Title}\"";
        }

        public void Close()
        {
            TryClose();
        }
    }
}
