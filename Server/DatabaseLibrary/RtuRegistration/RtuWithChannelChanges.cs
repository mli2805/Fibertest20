using System;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DatabaseLibrary
{
    public class RtuWithChannelChanges
    {
        public Guid RtuId { get; set; }
        public ChannelStateChanges MainChannel { get; set; } = ChannelStateChanges.TheSame;
        public ChannelStateChanges ReserveChannel { get; set; } = ChannelStateChanges.TheSame;

        public string Report()
        {
            var mainChannel = MainChannel == ChannelStateChanges.TheSame
                ? ""
                : MainChannel == ChannelStateChanges.Broken
                    ? "Main channel is Broken"
                    : "Main channel Recovered";

            var reserveChannel = ReserveChannel == ChannelStateChanges.TheSame
                ? ""
                : ReserveChannel == ChannelStateChanges.Broken
                    ? "Reserve channel is Broken"
                    : "Reserve channel Recovered";

            return $"RTU {RtuId.First6()} " + mainChannel + reserveChannel;
        }
    }
}