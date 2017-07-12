using Caliburn.Micro;
using ClientWcfServiceLibrary;
using System.ServiceModel;
using Iit.Fibertest.Utils35;

namespace WcfTestBench
{
    public class ShellViewModel : Screen, IShell
    {
        private IniFile _clientIniFile;
        private Logger35 _clientLog;
        internal static ServiceHost MyServiceHost;
        public ShellViewModel()
        {
            _clientIniFile = new IniFile();
            _clientIniFile.AssignFile("Client.ini");

            _clientLog = new Logger35();
            _clientLog.AssignFile("Client.log");

            StartWcf();

            // if there are more than one child view - delete this line
            WcfView();
        }

        private void StartWcf()
        {
            if (MyServiceHost != null)
            {
                MyServiceHost.Close();
            }
            ClientWcfService.ClientLog = _clientLog;
            MyServiceHost = new ServiceHost(typeof(ClientWcfService));
            MyServiceHost.Open();
        }

        public void WcfView()
        {
            var vm = new WcfClientViewModel(_clientIniFile, _clientLog);
            IWindowManager windowManager = new WindowManager();
            windowManager.ShowDialog(vm);
        }

    }
}