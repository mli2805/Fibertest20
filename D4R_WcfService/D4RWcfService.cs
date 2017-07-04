using System;

namespace D4R_WcfService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "D4R_WcfService" in both code and config file together.
    public class D4RWcfService : ID4RWcfService
    {
        public string GetData(int value)
        {
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
