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
            return DcManager.CheckRtuConnection(rtuAddress);
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
    }
}
