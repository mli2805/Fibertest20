using Caliburn.Micro;
using Iit.Fibertest.WpfCommonViews;

namespace WcfTester
{
    public class ShellViewModel : Caliburn.Micro.PropertyChangedBase, IShell
    {
        public void TraceState()
        {
            var vm = new TraceStateViewModel();
            IWindowManager windowManager = new WindowManager();
            windowManager.ShowWindow(vm);
        }
    }
}