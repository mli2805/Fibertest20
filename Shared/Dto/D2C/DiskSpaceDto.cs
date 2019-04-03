using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class DiskSpaceDto
    {
        [DataMember]
        public long TotalSize { get; set; }

        [DataMember]
        public long AvailableFreeSpace { get; set; }

        [DataMember]
        public long DataSize { get; set; }

        [DataMember]
        public long FreeSpaceThreshold { get; set; }
    }
}