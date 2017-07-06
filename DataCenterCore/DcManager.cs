using Dto;
using Iit.Fibertest.Utils35;

namespace DataCenterCore
{
    public class DcManager
    {
        private readonly Logger35 _dcLog;
        private readonly IniFile _coreIni;

        public DcManager()
        {
            _coreIni = new IniFile();
            _coreIni.AssignFile("DcCore.ini");
            var cultureString = _coreIni.Read(IniSection.General, IniKey.Culture, "ru-RU");

            _dcLog = new Logger35();
            _dcLog.AssignFile("DcCore.log", cultureString);
        }

        public bool InitializeRtu(InitializeRtu rtu)
        {
            _dcLog.AppendLine("rtu initialization");
            return true;
        }
    }
}
