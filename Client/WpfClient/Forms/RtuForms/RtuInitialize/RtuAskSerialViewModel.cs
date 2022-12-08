using Caliburn.Micro;

namespace Iit.Fibertest.Client
{
    public class RtuAskSerialViewModel : Screen
    {
        public string Message1 { get; set; }
        public string Message2 { get; set; }
        public string Message3 { get; set; }

        public string Serial { get; set; }

        public bool IsSavePressed;

        public void Initialize(bool isFirstInitialization, string address, string oldSerial)
        {
            IsSavePressed = false;

            Message1 = $"Unauthorized RTU {address} access.";
            Message2 = isFirstInitialization ? "First initialization" : "Probably RTU was changed";
            Message3 = "Please enter Platform serial number";

            Serial = oldSerial;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "";
        }

        public void Save()
        {
            IsSavePressed = true;
            TryClose();
        }

        public void Cancel()
        {
            IsSavePressed = false;
            TryClose();
        }
    }
}
