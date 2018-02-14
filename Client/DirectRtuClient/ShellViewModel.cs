using Caliburn.Micro;
using Iit.Fibertest.UtilsLib;

namespace DirectRtuClient
{
    public class ShellViewModel : PropertyChangedBase, IShell
    {

        public string IpAddress { get; set; }

        private readonly IMyLog _rtuLogger;
        private static IniFile _iniFile35;

        public ShellViewModel()
        {
            _iniFile35 = new IniFile();
            _iniFile35.AssignFile(@"rtu.ini");

            _rtuLogger = new LogFile(_iniFile35).AssignFile(@"rtu.log");

            IpAddress = _iniFile35.Read(IniSection.General, IniKey.OtauIp, @"172.16.5.53");

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
    }
}