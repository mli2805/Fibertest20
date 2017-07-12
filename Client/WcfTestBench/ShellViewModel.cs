using Caliburn.Micro;
using Iit.Fibertest.Utils35;

namespace WcfTestBench
{
    public class ShellViewModel : Caliburn.Micro.PropertyChangedBase, IShell
    {
        private IniFile _clientIniFile;
        private Logger35 _clientLog;
        public ShellViewModel()
        {
            _clientIniFile = new IniFile();
            _clientIniFile.AssignFile("Client.ini");

            _clientLog = new Logger35();
            _clientLog.AssignFile("Client.log");
        }


        public void WcfView()
        {
            var vm = new WcfClientViewModel(_clientIniFile, _clientLog);
            IWindowManager windowManager = new WindowManager();
            windowManager.ShowDialog(vm);
        }

    }
}