using System.Collections.Generic;

namespace Iit.Fibertest.Dto
{
    public class TraceInformationDto
    {
        public string TraceTitle;
        public string Port; // for trace on bop use bop's serial plus port number "879151-3"
        public string RtuTitle;

        public List<TraceInfoTableItem> Equipment;
        public List<TraceInfoTableItem> Nodes;

        public bool IsLightMonitoring;
        public string Comment;
    }
}