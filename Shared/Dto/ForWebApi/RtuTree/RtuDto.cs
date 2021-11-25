using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class RtuDto
    {
        [DataMember] public Guid RtuId;
        [DataMember] public RtuMaker RtuMaker;
        [DataMember] public string Title;
        // [DataMember] public string OtauId; // in VeEX RTU main OTAU has its own ID
        [DataMember] public string OtdrId; // ditto
        [DataMember] public VeexOtau MainVeexOtau = new VeexOtau(); // in Veex RTU it is a separate unit

        [DataMember] public string Mfid;
        [DataMember] public string Mfsn;
        [DataMember] public string Omid;
        [DataMember] public string Omsn;
        [DataMember] public string Serial;

        [DataMember] public int OwnPortCount;
        [DataMember] public int FullPortCount;
        [DataMember] public List<ChildDto> Children = new List<ChildDto>();

        [DataMember] public NetAddress MainChannel;
        [DataMember] public RtuPartState MainChannelState;
        [DataMember] public NetAddress ReserveChannel;
        [DataMember] public RtuPartState ReserveChannelState;
        [DataMember] public bool IsReserveChannelSet;
        [DataMember] public NetAddress OtdrNetAddress;
        [DataMember] public RtuPartState BopState;

        [DataMember] public MonitoringState MonitoringMode;

        [DataMember] public string Version;
        [DataMember] public string Version2;

    }
}
