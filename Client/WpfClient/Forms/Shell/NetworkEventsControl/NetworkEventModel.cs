using System;
using System.Windows.Media;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class NetworkEventModel
    {
        public int Ordinal { get; set; }
        public DateTime EventTimestamp { get; set; }
        public string RtuTitle { get; set; }
        public Guid RtuId { get; set; }

        private bool IsRtuAvailable => MainChannel == RtuPartState.Ok || ReserveChannel == RtuPartState.Ok;
        public string RtuAvailabilityString => IsRtuAvailable ? Resources.SID_Available : Resources.SID_Not_available;
        public Brush RtuAvailabilityBrush => GetAvailabilityBrush();

        public RtuPartState MainChannel { get; set; }
        public ChannelEvent OnMainChannel { get; set; }

        public string MainChannelStateString => OnMainChannel.ToLocalizedString();
        public Brush MainChannelStateBrush => OnMainChannel.GetBrush(false);

        public RtuPartState ReserveChannel { get; set; }
        public ChannelEvent OnReserveChannel { get; set; }
        public string ReserveChannelStateString => OnReserveChannel.ToLocalizedString();
        public Brush ReserveChannelStateBrush => OnReserveChannel.GetBrush(false);


        private Brush GetAvailabilityBrush()
        {
            if (MainChannel == RtuPartState.Ok && ReserveChannel != RtuPartState.Broken)
                return Brushes.Transparent;

            if ((int) OnMainChannel + (int) OnReserveChannel == 0)
                return Brushes.LightPink;

            return Brushes.Red;
        }
    }
}
