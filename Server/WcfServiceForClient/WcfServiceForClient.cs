using System.Diagnostics;
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
            var logLevel = ServiceIniFile.Read(IniSection.General, IniKey.LogLevel, 2);

            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            if (logLevel >= 2)
                ServiceLog?.AppendLine($"WcfServiceForClient listens port 11840 (process {pid}, thread {tid})");
        }


        public void RegisterClient(string address)
        {
            DcManager.RegisterClient(address);
        }

        public void UnRegisterClient(string address)
        {
            DcManager.UnRegisterClient(address);
        }

        public bool InitializeRtu(InitializeRtu rtu)
        {
            DcManager.InitializeRtu(rtu);
            return true;
        }
    }
}
