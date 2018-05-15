using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace LicenseMaker
{
    public class ShellViewModel : Screen, IShell
    {
        public License License { get; set; } = new License();
        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Fibertest 2.0 License maker";
        }

        public void SaveAsFile()
        {

        }

        public void Close()
        {
            TryClose();
        }
    }
}