using System;
using System.Windows.Media;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class BopNetworkEventModel
    {
        public int Nomer { get; set; }
        public DateTime EventTimestamp { get; set; }

        public string BopTitle { get; set; }
        public Guid BopId { get; set; }

        public string RtuTitle { get; set; }
        public Guid RtuId { get; set; }

        public RtuPartState State { get; set; }

        public string StateString => State.ToLocalizedString();
        public Brush StateBrush => State.GetBrush(false);

    }
}
