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
        }

        public bool ProcessRtuInitialized(RtuInitializedDto result)
        {
            return DcManager.ProcessRtuInitialized(result);
        }

        public bool ConfirmStartMonitoring(MonitoringStartedDto confirmation)
        {
            return DcManager.ConfirmStartMonitoring(confirmation);
        }

        public bool ConfirmStopMonitoring(MonitoringStoppedDto confirmation)
        {
            return DcManager.ConfirmStopMonitoring(confirmation);
        }

        public bool ProcessMonitoringResult(MonitoringResult result)
        {
            return DcManager.ProcessMonitoringResult(result);
        }
    }
}
