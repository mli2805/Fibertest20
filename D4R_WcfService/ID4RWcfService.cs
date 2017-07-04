using System.Runtime.Serialization;
using System.ServiceModel;

namespace D4R_WcfService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "ID4R_WcfService" in both code and config file together.
    [ServiceContract]
    public interface ID4RWcfService
    {
        [OperationContract]
        string GetData(int value);

        [OperationContract]
        CompositeType GetDataUsingDataContract(CompositeType composite);

        // TODO: Add your service operations here
    }

    // Use a data contract as illustrated in the sample below to add composite types to service operations.
    [DataContract]
    public class CompositeType
    {
        bool _boolValue = true;
        string _stringValue = "Hello ";

        [DataMember]
        public bool BoolValue
        {
            get { return _boolValue; }
            set { _boolValue = value; }
        }

        [DataMember]
        public string StringValue
        {
            get { return _stringValue; }
            set { _stringValue = value; }
        }
    }
}
