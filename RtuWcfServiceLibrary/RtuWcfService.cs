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
            Thread rtuManagerThread = new Thread(_rtuManager.Start);
            rtuManagerThread.Start();
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
    }
}
