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

        public static Thread RtuManagerThread { get; set; }

        private int _logLevel;
        private RtuManager _rtuManager;

        public RtuWcfService()
        {
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;

            _logLevel = WcfIniFile.Read(IniSection.General, IniKey.LogLevel, 2);
            if (_logLevel >=4)
                WcfLogger35?.AppendLine($"RtuWcfService started in process {pid}, thread {tid}");

            _rtuManager = new RtuManager();
            RtuManagerThread = new Thread(_rtuManager.Initialize);
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
            WcfLogger35.AppendLine("user asks to start monitoring");
            RtuManagerThread?.Abort();
            RtuManagerThread = new Thread(_rtuManager.StartMonitoring);
            RtuManagerThread.Start();
        }

        public void StopMonitoring()
        {
            WcfLogger35.AppendLine("user asks to stop monitoring");
            _rtuManager.StopMonitoring();
        }

    }
}
