using System.Globalization;
using System.Threading;
using Caliburn.Micro;
using Iit.Fibertest.Utils35;

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

            var culture = _clientIniFile.Read(IniSection.General, IniKey.Culture, "ru-RU");
            Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);

            // if there are more than one child view - delete this line
            WcfView();
        }

        public void WcfView()
        {
            var vm = new WcfClientViewModel(_clientIniFile, _clientLog);
            IWindowManager windowManager = new WindowManager();
            windowManager.ShowDialog(vm);
        }


    }
}