using System.Threading;
using DataCenterCore;
using Dto;
using Iit.Fibertest.Utils35;

namespace WcfServiceForClient
{
    public class WcfServiceForClient : IWcfServiceForClient
    {
        public static IniFile ServiceIniFile { get; set; }
        public static Logger35 ServiceLog { get; set; }
        public static DcManager DcManager { get; set; }


        public WcfServiceForClient()
        {
            if (ServiceIniFile == null)
            {
                ServiceIniFile = new IniFile();
                ServiceIniFile.AssignFile(@"WcfIniFile");
            }
        }


        public void RegisterClient(string address)
        {
            DcManager.RegisterClient(address);
        }

        public void UnRegisterClient(string address)
        {
            DcManager.UnRegisterClient(address);
        }

        public bool CheckRtuConnection(CheckRtuConnectionDto rtuAddress)
        {
            var thread = new Thread(DcManager.CheckRtuConnection) { IsBackground = true };
            thread.Start(rtuAddress);
            return true;
        }

        public bool InitializeRtu(InitializeRtuDto rtu)
        {
            DcManager.InitializeRtu(rtu);
            return true;
        }

        public bool StartMonitoring(string rtuAddress)
        {
            return DcManager.StartMonitoring(rtuAddress);
        }

        public bool StopMonitoring(string rtuAddress)
        {
            return DcManager.StopMonitoring(rtuAddress);
        }

        public bool ApplyMonitoringSettings(ApplyMonitoringSettingsDto settings)
        {
            return DcManager.ApplyMonitoringSettings(settings);
        }

        public bool AssignBaseRef(AssignBaseRefDto baseRef)
        {
            return DcManager.AssignBaseRef(baseRef);
        }
    }
}
