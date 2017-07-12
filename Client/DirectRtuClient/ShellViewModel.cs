using Caliburn.Micro;
using Iit.Fibertest.Utils35;
using System.ServiceModel;
using Client_WcfService;
using DirectRtuClient;

namespace RtuWpfExample
{
    public class ShellViewModel : PropertyChangedBase, IShell
    {

        public string IpAddress { get; set; }

        private readonly Logger35 _rtuLogger;
        private readonly Logger35 _clientLog;
        private static IniFile _iniFile35;

        internal static ServiceHost myServiceHost = null;
        public ShellViewModel()
        {
            _rtuLogger = new Logger35();
            _rtuLogger.AssignFile("rtu.log");

            _iniFile35 = new IniFile();
            _iniFile35.AssignFile("rtu.ini");

            _clientLog = new Logger35();
            _clientLog.AssignFile(@"Client.log");



            IpAddress = _iniFile35.Read(IniSection.General, IniKey.OtauIp, "192.168.96.53");

            StartWcf();

        }

        private void StartWcf()
        {
            if (myServiceHost != null)
            {
                myServiceHost.Close();
            }
            ClientWcfService.ClientLog = _clientLog;
            myServiceHost = new ServiceHost(typeof(ClientWcfService));
            myServiceHost.Open();
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