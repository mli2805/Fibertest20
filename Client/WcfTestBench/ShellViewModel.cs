using System.Globalization;
using System.Threading;
using Caliburn.Micro;
using Iit.Fibertest.UtilsLib;

namespace WcfTestBench
{
    public class ShellViewModel : Screen, IShell
    {
        private IniFile _clientIniFile;
        private IMyLog _clientLog;

        public ShellViewModel()
        {
            _clientIniFile = new IniFile();
            _clientIniFile.AssignFile("Client.ini");
            var culture = _clientIniFile.Read(IniSection.General, IniKey.Culture, "ru-RU");

            _clientLog = new LogFile(_clientIniFile).AssignFile("Client.log");

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