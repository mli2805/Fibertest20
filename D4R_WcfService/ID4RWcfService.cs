using System;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace D4R_WcfService
{
    [ServiceContract]
    public interface ID4RWcfService
    {
        [OperationContract]
        void ConfirmInitilization(RtuInitializationResult result);

        [OperationContract]
        void SendMonitoringResult(MonitoringResult result);
    }

    [DataContract]
    public class MonitoringResult
    {
        [DataMember]
        public Guid RtuId { get; set; }

        [DataMember]
        public byte[] SorData { get; set; }
    }

    [DataContract]
    public class RtuInitializationResult
    {
        [DataMember]
        public Guid RtuId { get; set; }

        [DataMember]
        public bool IsInitialized { get; set; }

        [DataMember]
        public string Serial { get; set; }

        [DataMember]
        public int OwnPortCount { get; set; }

        [DataMember]
        public int FullPortCount { get; set; }
    }

   
}
