using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.DatabaseLibrary
{
    public enum Channel { Main, Reserve }
    public class RtuWithChannelChangesList
    {
        public List<RtuWithChannelChanges> List = new List<RtuWithChannelChanges>();

        public void AddOrUpdate(RtuStation rtuStation, Channel channel, ChannelStateChanges changes)
        {
            var rtu = List.FirstOrDefault(r => r.RtuId == rtuStation.RtuGuid);
            if (rtu == null)
            {
                var rtuWithChannelChanges = new RtuWithChannelChanges() { RtuId = rtuStation.RtuGuid };
                if (channel == Channel.Main)
                    rtuWithChannelChanges.MainChannel = changes;
                else
                    rtuWithChannelChanges.ReserveChannel = changes;
                List.Add(rtuWithChannelChanges);
            }
            else
            {
                if (channel == Channel.Main)
                    rtu.MainChannel = changes;
                else
                    rtu.ReserveChannel = changes;
            }
        }
    }
}