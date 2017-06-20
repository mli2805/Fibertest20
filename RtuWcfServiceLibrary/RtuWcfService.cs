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

        public string GetData(int value)
        {
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;

            WcfLogger35.AppendLine($"GetData request received. Process {pid}, thread {tid}");
            return string.Format("You entered: {0}", value);  
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
