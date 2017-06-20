using System;
using System.Diagnostics;
using System.Threading;
using Iit.Fibertest.Utils35;

namespace RtuWcfServiceLibrary
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "RtuWcfService" in both code and config file together.
    public class RtuWcfService : IRtuWcfService
    {
        public static Logger35 WcfLogger35 { get; set; }

        public RtuWcfService()
        {
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;

            
            WcfLogger35?.AppendLine($"RtuWcfService started in process {pid}, thread {tid}");
        }

        public string GetData(int value)
        {
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
