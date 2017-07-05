using System.Diagnostics;
using System.Threading;
using Iit.Fibertest.Utils35;

namespace D4R_WcfService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "D4R_WcfService" in both code and config file together.
    public class D4RWcfService : ID4RWcfService
    {
        public static IniFile ServiceIniFile { get; set; }
        public static Logger35 ServiceLog { get; set; }

        public D4RWcfService()
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
                ServiceLog?.AppendLine($"D4RWcfService started in process {pid}, thread {tid}");
        }

        public void ConfirmInitilization(RtuInitializationResult result)
        {
            
        }

        public void SendMonitoringResult(MonitoringResult result)
        {
            ServiceLog.AppendLine($"Monitoring result received. Sor size is {result.SorData.Length}");
        }
    }
}
