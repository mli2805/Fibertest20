using Caliburn.Micro;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class RtuAskSerialViewModel : Screen
    {
        public string Message1 { get; set; }
        //public string Message2 { get; set; }
        public string Message3 { get; set; }

        public string Serial { get; set; }

        public bool IsSavePressed;

        public void Initialize(bool isFirstInitialization, string address, string oldSerial)
        {
            IsSavePressed = false;

            Message1 = string.Format(Resources.SID_Unauthorized_access_to_RTU__0__, address);
            //Message2 = isFirstInitialization ? "First initialization" : "Probably RTU was changed";
            Message3 = Resources.SID_Please_enter_Platform_serial_number;

            Serial = oldSerial;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "";
        }

        public void Continue()
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
