using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class DoClientMeasurementDto
    {
        [DataMember]
        public string ConnectionId { get; set; }
        [DataMember]
        public string ClientIp { get; set; }

        [DataMember]
        public Guid RtuId { get; set; }

        [DataMember]
        public List<MeasParamByPosition> SelectedMeasParams { get; set; }
        [DataMember]
        public VeexMeasOtdrParameters VeexMeasOtdrParameters { get; set; }
        [DataMember]
        public AnalysisParameters AnalysisParameters { get; set; }

        [DataMember]
        public List<List<OtauPortDto>> OtauPortDtoList { get; set; }

        [DataMember]
        public string OtdrId { get; set; }

        // true - apply semi-analysis (only start/end and one section between them (for auto base ref mode)
        // false - apply full auto analysis (usual client measurement)
        [DataMember]
        public bool IsForAutoBase { get; set; }

        [DataMember]
        public bool IsAutoLmax { get; set; }
    }

}