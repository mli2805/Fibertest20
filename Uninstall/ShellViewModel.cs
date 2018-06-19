using Caliburn.Micro;

namespace Uninstall
{
    public class ShellViewModel : Screen, IShell
    {
        public string MainName = "IIT Fibertest 2.0";
        public HeaderViewModel HeaderViewModel { get; set; }
        public UnInstallFolderViewModel UnInstallFolderViewModel { get; set; }

        public ShellViewModel()
        {
            HeaderViewModel = new HeaderViewModel();
            HeaderViewModel.InBold = $"Uninstall {MainName}";
            HeaderViewModel.Explanation = $"Remove {MainName} from your computer";

            UnInstallFolderViewModel = new UnInstallFolderViewModel();
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