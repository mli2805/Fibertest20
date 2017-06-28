using System;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading;
using Iit.Fibertest.Utils35;
using RtuManagement;

namespace RtuWcfServiceLibrary
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "RtuWcfService" in both code and config file together.
    [ServiceBehavior (InstanceContextMode = InstanceContextMode.Single)]
    public class RtuWcfService : IRtuWcfService
    {
        public static IniFile WcfIniFile { get; set; }
        public static Logger35 WcfLogger35 { get; set; }

        private static Thread RtuManagerThread { get; set; }

        private readonly int _logLevel;
        private readonly RtuManager _rtuManager;

        public RtuWcfService()
        {
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;

            _logLevel = WcfIniFile.Read(IniSection.General, IniKey.LogLevel, 2);
            if (_logLevel >=2)
                WcfLogger35?.AppendLine($"RtuWcfService started in process {pid}, thread {tid}");

            _rtuManager = new RtuManager(WcfLogger35, WcfIniFile);
            RtuManagerThread = new Thread(_rtuManager.Initialize) {IsBackground = true};
            RtuManagerThread.Start();
        }

        public string GetData(int value)
        {
            if (_logLevel >=4)
                WcfLogger35.AppendLine("GetData request received");
            return $"You entered: {value}";  
        }

        public CompositeType GetDataUsingDataContract(CompositeType composite)
        {
            if (composite == null)
            {
                throw new ArgumentNullException("composite");
            }
            if (composite.BoolValue)
            {
                composite.StringValue += "Suffix";
            }
            return composite;
        }

        public void StartMonitoring()
        {
            WcfLogger35.AppendLine("User asks to start monitoring");
            if (_rtuManager.IsMonitoringOn)
            {
                WcfLogger35.AppendLine("Rtu is in AUTOMATIC mode already");
                return;
            }
            RtuManagerThread?.Abort();
            RtuManagerThread = new Thread(_rtuManager.StartMonitoring);
            RtuManagerThread.IsBackground = true;
            RtuManagerThread.Start();
//            _rtuManager.StartMonitoring();
        }

        public void StopMonitoring()
        {
            WcfLogger35.AppendLine("User asks to stop monitoring");
            if (!_rtuManager.IsMonitoringOn)
            {
                WcfLogger35.AppendLine("Rtu is in MANUAL mode already");
                return;
            }
            _rtuManager.StopMonitoring();
        }

    }
}
