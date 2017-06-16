using System;
using System.Diagnostics;
using System.Threading;
using Iit.Fibertest.Utils35;

namespace RtuWcfServiceLibrary
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "RtuWcfService" in both code and config file together.
    public class RtuWcfService : IRtuWcfService
    {
        private Logger35 _wcfLogger35 = null;

        public RtuWcfService()
        {
            //            _wcfLogger35 = new Logger35();
            //            _wcfLogger35.AssignFile("wcf.log");
            //            _wcfLogger35.AppendLine("Wcf service started");
        }

        public string GetData(int value)
        {
            if (_wcfLogger35 == null)
            {
                _wcfLogger35 = new Logger35();
                _wcfLogger35.AssignFile("RtuService.log");
            }

            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;

            _wcfLogger35.AppendLine($"GetData request received. Process {pid}, thread {tid}");
            _wcfLogger35.CloseFile();
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
