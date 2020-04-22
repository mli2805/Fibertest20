using System.Collections.Generic;

namespace Iit.Fibertest.Dto
{
    public class TraceInformationDto
    {
        public string TraceTitle;

        // for detached trace "-1";
        // for trace on bop use bop's serial plus port number "879151-3"
        public string Port; 
        
        public string RtuTitle;

        public List<TraceInfoTableItem> Equipment;
        public List<TraceInfoTableItem> Nodes;

        public bool IsLightMonitoring;
        public string Comment;
    }
}