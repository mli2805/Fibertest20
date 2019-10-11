using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class ChildDto
    {
        [DataMember] public int Port;
        [DataMember] public ChildType ChildType;

        public ChildDto(ChildType childType)
        {
            ChildType = childType;
        }
    }
}