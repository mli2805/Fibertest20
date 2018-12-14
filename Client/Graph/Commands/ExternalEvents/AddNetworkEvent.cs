using System;
using System.Windows.Media;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;


namespace Iit.Fibertest.Graph
{
    public enum ChannelEvent
    {
        Broken = -1,
        Nothing,
        Repaired,
    }

    public static class ChannelEventExt
    {
        public static RtuPartState ChangeChannelState(this ChannelEvent channelEvent, RtuPartState previousState)
        {
            if (channelEvent == ChannelEvent.Nothing) return previousState;
            return channelEvent == ChannelEvent.Broken ? RtuPartState.Broken : RtuPartState.Ok;
        }
        public static string ToLocalizedString(this ChannelEvent state)
        {
            switch (state)
            {
                case ChannelEvent.Broken:
                    return Resources.SID_Broken;
                case ChannelEvent.Nothing:
                    return "";
                case ChannelEvent.Repaired:
                    return Resources.SID_Ok;
                default: return "";
            }
        }

        public static Brush GetBrush(this ChannelEvent state, bool isForeground)
        {
            switch (state)
            {
                case ChannelEvent.Broken:
                    return Brushes.Red;
                case ChannelEvent.Nothing:
                    return isForeground ? Brushes.LightGray : Brushes.Transparent;
                case ChannelEvent.Repaired:
                    return isForeground ? Brushes.Black : Brushes.Transparent;
                default:
                    return isForeground ? Brushes.Black : Brushes.Transparent;
            }
        }
    }

    public class AddNetworkEvent
    {
        public DateTime EventTimestamp { get; set; }
        public Guid RtuId { get; set; }
        public ChannelEvent OnMainChannel { get; set; }
        public ChannelEvent OnReserveChannel { get; set; }
    }
}