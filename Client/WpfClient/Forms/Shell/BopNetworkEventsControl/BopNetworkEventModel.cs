using System;
using System.Windows.Media;
using Iit.Fibertest.Dto;

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

        public string StateString => State.GetLocalizedString();
        public Brush StateBrush => State.GetBrush();

    }
}
