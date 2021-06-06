using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class DiskSpaceDto
    {
        [DataMember]
        public double TotalSize { get; set; }

        [DataMember]
        public double AvailableFreeSpace { get; set; }

        [DataMember]
        public double DataSize { get; set; }

        [DataMember]
        public double FreeSpaceThreshold { get; set; }
    }
}