using Caliburn.Micro;
using Iit.Fibertest.UtilsLib;

namespace DbMigratorWpf
{
    public class ShellViewModel : PropertyChangedBase, IShell
    {
        private readonly IMyLog _logFile;
        private static IniFile _iniFile;


        public ShellViewModel()
        {
            _iniFile = new IniFile();
            _iniFile.AssignFile(@"migrator.ini");

            _logFile = new LogFile(_iniFile).AssignFile(@"rtu.log");
        }
        public void BtnSend()
        {

        }

      
    }

    public class Wcf
    {
        public void GetChannel()
        {

        }
    }
}