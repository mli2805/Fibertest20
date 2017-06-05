using Caliburn.Micro;
using Iit.Fibertest.RtuWpfExample;
using Iit.Fibertest.Utils35;

namespace RtuWpfExample
{
    public class ShellViewModel : PropertyChangedBase, IShell
    {
        
        public string IpAddress { get; set; }

        private readonly Logger35 _rtuLogger;
        private static IniFile _iniFile35;

        public ShellViewModel()
        {
            _rtuLogger = new Logger35();
            _rtuLogger.AssignFile("rtu.log");

            _iniFile35 = new IniFile();
            _iniFile35.AssignFile("rtu.ini");

            IpAddress = _iniFile35.Read(IniSection.General, IniKey.OtauIp, "192.168.96.53");

        }

        public void OtdrView()
        {
            var vm = new OtdrViewModel(_iniFile35, _rtuLogger, IpAddress);
            IWindowManager windowManager = new WindowManager();
            windowManager.ShowWindow(vm);
        }
        public void OtauView()
        {
            var otauPort = _iniFile35.Read(IniSection.General, IniKey.OtauPort, 23);
            var vm = new OtauViewModel(IpAddress, otauPort, _iniFile35, _rtuLogger);
            IWindowManager windowManager = new WindowManager();
            windowManager.ShowWindow(vm);
        }

        public void WcfView()
        {
            var vm = new WcfClientViewModel();
            IWindowManager windowManager = new WindowManager();
            windowManager.ShowDialog(vm);
        }


    }
}