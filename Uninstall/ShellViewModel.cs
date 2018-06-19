using Caliburn.Micro;

namespace Uninstall
{
    public class ShellViewModel : Screen, IShell
    {
        public string MainName = "IIT Fibertest 2.0";
        public HeaderViewModel HeaderViewModel { get; set; }
        public string Text1 { get; set; } 

        public ShellViewModel()
        {
            HeaderViewModel = new HeaderViewModel();
            HeaderViewModel.InBold = $"Uninstall {MainName}";
            HeaderViewModel.Explanation = $"Remove {MainName} from your computer";

            Text1 = $"{MainName} will be uninstalled from the following folder. Click Uninstall to start the uninstallation.";
        }

        public void Uninstall()
        {

        }

        public void Cancel()
        {
            TryClose();
        }
    }
}