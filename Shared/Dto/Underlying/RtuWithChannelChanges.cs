using System;

namespace Iit.Fibertest.Dto
{
    public class RtuWithChannelChanges
    {
        public Guid RtuId { get; set; }
        public RtuPartState MainChannel { get; set; } = RtuPartState.NotSetYet;
        public RtuPartState ReserveChannel { get; set; } = RtuPartState.NotSetYet;

        public string Report()
        {
            var mainChannel = MainChannel == RtuPartState.Broken
                    ? "Main channel is Broken"
                    : "Main channel Recovered";

            var reserveChannel = ReserveChannel == RtuPartState.NotSetYet
                ? ""
                : ReserveChannel == RtuPartState.Broken
                    ? "Reserve channel is Broken"
                    : "Reserve channel Recovered";

            return $"RTU {RtuId.First6()} " + mainChannel + reserveChannel;
        }
    }
}