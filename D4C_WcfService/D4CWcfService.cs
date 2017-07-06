using System.Diagnostics;
using System.Threading;
using DataCenterCore;
using Dto;
using Iit.Fibertest.Utils35;

namespace D4C_WcfService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Id4CWcfService" in both code and config file together.
    public class D4CWcfService : ID4CWcfService
    {
        public static IniFile ServiceIniFile { get; set; }
        public static Logger35 ServiceLog { get; set; }
        public static DcManager DcManager { get; set; }


        public D4CWcfService()
        {
            if (ServiceIniFile == null)
            {
                ServiceIniFile = new IniFile();
                ServiceIniFile.AssignFile(@"WcfIniFile");
            }
            var logLevel = ServiceIniFile.Read(IniSection.General, IniKey.LogLevel, 2);

            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            if (logLevel >= 2)
                ServiceLog?.AppendLine($"D4CWcfService started in process {pid}, thread {tid}");
        }


        public bool InitializeRtu(InitializeRtu rtu)
        {
            DcManager.InitializeRtu(rtu);
            return true;
        }
    }
}
