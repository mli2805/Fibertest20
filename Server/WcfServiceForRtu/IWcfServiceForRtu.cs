using System;
using System.Runtime.Serialization;
using System.ServiceModel;
using Dto;
using Iit.Fibertest.Utils35;

namespace WcfServiceForRtu
{
    [ServiceContract]
    public interface IWcfServiceForRtu
    {
        [OperationContract]
        void ConfirmInitilization(RtuInitialized result);

        [OperationContract]
        void SendMonitoringResult(MonitoringResult result);
    }

    [DataContract]
    public class MonitoringResult
    {
        [DataMember]
        public Guid RtuId { get; set; }

        [DataMember]
        public BaseRefType BaseRefType { get; set; }

        [DataMember]
        public byte[] SorData { get; set; }
    }

   
}
