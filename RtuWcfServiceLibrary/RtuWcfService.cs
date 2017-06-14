using System;
using Iit.Fibertest.Utils35;

namespace RtuWcfServiceLibrary
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "RtuWcfService" in both code and config file together.
    public class RtuWcfService : IRtuWcfService
    {
//        private readonly Logger35 _logger35;

//        public RtuWcfService(Logger35 logger35)
//        {
//            _logger35 = logger35;
//            _logger35.AppendLine("Wcf service started");
//        }

        public string GetData(int value)
        {
//            _logger35.AppendLine($"{value}");
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
