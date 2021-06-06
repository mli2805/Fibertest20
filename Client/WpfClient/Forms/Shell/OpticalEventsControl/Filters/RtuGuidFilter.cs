using System;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class RtuGuidFilter
    {
        public bool IsOn { get; set; }
        public Guid RtuId { get; set; }
        public string RtuTitle { get; set; }

        public RtuGuidFilter() { IsOn = false; }

        public RtuGuidFilter(Guid rtuId, string rtuTitle)
        {
            IsOn = true;
            RtuId = rtuId;
            RtuTitle = rtuTitle;
        }

        public override string ToString()
        {
            return IsOn ? RtuTitle : Resources.SID__no_filter_;
        }
    }
}