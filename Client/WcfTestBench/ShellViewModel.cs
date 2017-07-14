using Caliburn.Micro;
using System.ServiceModel;
using Iit.Fibertest.Utils35;
using Iit.Fibertest.WpfCommonViews;
using WcfTestBench.MonitoringSettings;

namespace WcfTestBench
{
    public class ShellViewModel : Screen, IShell
    {
        private IniFile _clientIniFile;
        private Logger35 _clientLog;

        public ShellViewModel()
        {
            _clientIniFile = new IniFile();
            _clientIniFile.AssignFile("Client.ini");

            _clientLog = new Logger35();
            _clientLog.AssignFile("Client.log");

            // if there are more than one child view - delete this line
            Temp();
        }

        public void WcfView()
        {
            var vm = new WcfClientViewModel(_clientIniFile, _clientLog);
            IWindowManager windowManager = new WindowManager();
            windowManager.ShowDialog(vm);
        }

        public void Temp()
        {
            var vm = new MonitoringSettingsViewModel();
            IWindowManager windowManager = new WindowManager();
            windowManager.ShowDialog(vm);

        }

    }
}