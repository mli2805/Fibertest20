using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class ReactOltTrapDto
    {
        [DataMember]
        public string OltIp { get; set; }

        [DataMember]
        public int Port { get; set; }
    }
}