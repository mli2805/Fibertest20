using Iit.Fibertest.UtilsLib;
using Utils471;

namespace Broadcaster {
    public class ShellViewModel : Caliburn.Micro.PropertyChangedBase, IShell
    {
        public GsmViewModel GsmViewModel { get; set; }
        public MsmqViewModel MsmqViewModel { get; set; }
        public SnmpViewModel SnmpViewModel { get; set; }
        public HuaweiTrapViewModel HuaweiTrapViewModel { get; set; }

        public ShellViewModel()
        {
            var iniFile = new IniFile();
            iniFile.AssignFile("broadcaster.ini");
            var logFile = new LogFile(iniFile);
            logFile.AssignFile("broadcaster.log");

            GsmViewModel = new GsmViewModel(iniFile);
            MsmqViewModel = new MsmqViewModel(iniFile);
            SnmpViewModel = new SnmpViewModel(new SnmpService());
            HuaweiTrapViewModel = new HuaweiTrapViewModel();
        }
    }
}