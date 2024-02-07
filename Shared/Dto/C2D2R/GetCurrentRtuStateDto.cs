using System;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class GetCurrentRtuStateDto
    {
        [DataMember]
        public string ClientIp { get; set; }
        [DataMember]
        public string ConnectionId { get; set; }


        // When client polling RTU for initialization result - client should fill RtuDoubleAddress from Initialization view
        // When data-center polling RTU - polling thread fills RtuDoubleAddress from WriteModel

        [DataMember]
        public DoubleAddress RtuDoubleAddress { get; set; }


        // Server says RTU that last fetched measurement has this timestamp
        // and all monitoring results older than this could be removed from db
        [DataMember]
        public DateTime LastMeasurementTimestamp { get; set; }
    }
}