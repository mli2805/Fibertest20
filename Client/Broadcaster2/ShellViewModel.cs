using Iit.Fibertest.UtilsLib;

namespace Broadcaster2 {
    public class ShellViewModel : Caliburn.Micro.PropertyChangedBase, IShell
    {
        public GsmViewModel GsmViewModel { get; set; }

        public ShellViewModel()
        {
            var iniFile = new IniFile();
            iniFile.AssignFile("broadcaster.ini");

            GsmViewModel = new GsmViewModel(iniFile);
        }
    }
}