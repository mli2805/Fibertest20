using System;
using System.Windows.Media;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
// ReSharper disable InconsistentNaming

namespace Iit.Fibertest.Client
{
    public class NetworkEventModel
    {
        public int Ordinal { get; set; }
        public DateTime EventTimestamp { get; set; }
        public string RtuTitle { get; set; }
        public Guid RtuId { get; set; }

        public bool IsRtuAvailable;
        public string RtuAvailabilityString => IsRtuAvailable ? Resources.SID_Available : Resources.SID_Not_available;
        public Brush RtuAvailabilityBrush => GetAvailabilityBrush();

        public RtuPartState MainChannel { get; set; }
        public ChannelEvent OnMainChannel { get; set; }

        public string MainChannelEventString => OnMainChannel == ChannelEvent.Nothing 
            ? MainChannel.ToLocalizedString() 
            : OnMainChannel.ToLocalizedString();

        // выбираем цвет фона исходя из события в канале -
        // если была авария и осталась авария, то не произошло ничего - прозрачный фон
        //public Brush MainChannelEventBrush => OnMainChannel.GetBrush(false);

        // 06/02/2026 вернул по требованию Хазанова
        // выбираем цвет фона исходя из состояния канала -
        // если была авария и осталась авария  - красный фон
        public Brush MainChannelEventBrush => MainChannel.GetBrush(false);

        public RtuPartState ReserveChannel { get; set; }
        public ChannelEvent OnReserveChannel { get; set; }
        public string ReserveChannelEventString => OnReserveChannel == ChannelEvent.Nothing 
            ? ReserveChannel.ToLocalizedString() 
            : OnReserveChannel.ToLocalizedString();


        //public Brush ReserveChannelEventBrush => OnReserveChannel.GetBrush(false);
        public Brush ReserveChannelEventBrush => ReserveChannel.GetBrush(false);


        private Brush GetAvailabilityBrush()
        {
            if (MainChannel == RtuPartState.Ok && ReserveChannel != RtuPartState.Broken)
                return Brushes.Transparent;

            if (((int) MainChannel + (int) ReserveChannel) == 0)
                return Brushes.LightPink;

            return Brushes.Red;
        }
    }
}
