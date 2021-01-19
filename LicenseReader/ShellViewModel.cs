using Caliburn.Micro;

namespace LicenseReader 
{
    public class ShellViewModel : Screen, IShell
    {

        public void Close()
        {
            TryClose();
        }
    }
}