using Caliburn.Micro;

namespace SuperClient 
{
    public class ShellViewModel : Caliburn.Micro.PropertyChangedBase, IShell
    {
        public void Start()
        {
            var vm = new SecondLevelViewModel();
            var wm = new WindowManager();
            wm.ShowDialog(vm);
        }
    }
}