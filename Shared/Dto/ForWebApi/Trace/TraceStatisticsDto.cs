using System.Collections.Generic;

namespace Iit.Fibertest.Dto
{
    public class TraceStatisticsDto
    {
        public string TraceTitle;
        public string Port; // for trace on bop use bop's serial plus port number "879151-3"
        public string RtuTitle;
        public List<BaseRefInfoDto> BaseRefs;
        public List<MeasurementDto> Measurements;
    }
}