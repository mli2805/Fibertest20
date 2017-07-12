using System.Diagnostics;
using System.Threading;
using DataCenterCore;
using Dto;
using Iit.Fibertest.Utils35;

namespace WcfServiceForRtu
{
    public class WcfServiceForRtu : IWcfServiceForRtu
    {
        public static IniFile ServiceIniFile { get; set; }
        public static Logger35 ServiceLog { get; set; }

        public static DcManager DcManager { get; set; }

        public WcfServiceForRtu()
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
                ServiceLog?.AppendLine($"WcfServiceForRtu listen port 11841 (process {pid}, thread {tid})");
        }

        public void ConfirmInitilization(RtuInitialized result)
        {
            var str = result.IsInitialized ? "OK" : "ERROR";
            ServiceLog.AppendLine($"Rtu {result.Id} initialization {str}");
            DcManager.ConfirmRtuInitialized(result);
        }

        public void SendMonitoringResult(MonitoringResult result)
        {
            ServiceLog.AppendLine($"Monitoring result received. Sor size is {result.SorData.Length}");
        }
    }
}
