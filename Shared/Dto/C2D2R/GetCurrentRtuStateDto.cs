using System;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class GetCurrentRtuStateDto
    {
        // only if RtuId matches with one persisted on RTU - server has right to get current state
        [DataMember]
        public Guid RtuId { get; set; }

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