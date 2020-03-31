using System.Collections.Generic;

namespace Iit.Fibertest.Dto
{
    public class RtuMonitoringSettingsDto
    {
        public string RtuTitle;
        public RtuMaker RtuMaker;
        public MonitoringState MonitoringMode;

        public Frequency PreciseMeas;
        public Frequency PreciseSave;
        public Frequency FastSave;

        public List<RtuMonitoringPortDto> Lines;
    }
}