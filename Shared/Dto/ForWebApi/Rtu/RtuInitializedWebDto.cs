using System;

namespace Iit.Fibertest.Dto
{
    public class RtuInitializedWebDto
    {
        public Guid RtuId { get; set; }

        public string RequestId { get; set; }
        public ReturnCode ReturnCode { get; set; }
        public string ErrorMessage { get; set; }

        public RtuNetworkSettingsDto RtuNetworkSettings { get; set; }
    }
}