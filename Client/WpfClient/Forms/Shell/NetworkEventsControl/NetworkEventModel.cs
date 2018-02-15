using System;
using System.Windows.Media;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class NetworkEventModel
    {
        public int Nomer { get; set; }
        public DateTime EventTimestamp { get; set; }
        public string RtuTitle { get; set; }
        public Guid RtuId { get; set; }

        private bool IsRtuAvailable => MainChannelState == RtuPartState.Ok || ReserveChannelState == RtuPartState.Ok;
        public string RtuAvailabilityString => IsRtuAvailable ? Resources.SID_Available : Resources.SID_Not_available;
        public Brush RtuAvailabilityBrush => GetAvailabilityBrush();

        public RtuPartState MainChannelState { get; set; }

        public string MainChannelStateString => MainChannelState.ToLocalizedString();
        public Brush MainChannelStateBrush => MainChannelState.GetBrush(false);

        public RtuPartState ReserveChannelState { get; set; }
        public string ReserveChannelStateString => ReserveChannelState.ToLocalizedString();
        public Brush ReserveChannelStateBrush => ReserveChannelState.GetBrush(false);


        private Brush GetAvailabilityBrush()
        {
            if (MainChannelState == RtuPartState.Ok && ReserveChannelState != RtuPartState.Broken)
                return Brushes.Transparent;

            if ((int) MainChannelState + (int) ReserveChannelState == 0)
                return Brushes.LightPink;

            return Brushes.Red;
        }
    }
}
