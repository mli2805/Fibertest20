using System.Collections.Generic;
using System.Windows.Media;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public class RtuStateVm
    {
        public string Title { get; set; }
        public string RtuAvailabilityOnScreen => RtuAvailabilityToString();
        public Brush RtuAvailabilityBrush => RtuAvalilabilityToBrush(true);

        public RtuPartState MainAddressState { get; set; }
        public string MainAddress { get; set; }
        public string MainAddressStateOnScreen => MainAddressState.ToLocalizedString();

        public bool HasReserveAddress { get; set; }
        public RtuPartState ReserveAddressState { get; set; }
        public string ReserveAddress { get; set; }
        public string ReserveAddressStateOnScreen => ReserveAddressState.ToLocalizedString();

        public int FullPortCount { get; set; }
        public int OwnPortCount { get; set; }
        public string PortCountOnScreen => $@"{OwnPortCount}/{FullPortCount}";
        public int ActivePort { get; set; }
        public string ActivePortOnScreen => ActivePort < 1 ? "unknown" : $@"{ActivePort}";

        public int TraceCount { get; set; }
        public int BopCount { get; set; }

        public bool HasBop { get; set; }
        public string BopState { get; set; }

        public string TracesState { get; set; }
        public string MonitoringMode { get; set; }

        public string CurrentMeasurementStep { get; set; }

        public List<PortLineVm> Ports { get; set; }

        //------------------
        private string RtuAvailabilityToString()
        {
            return MainAddressState == RtuPartState.Normal || ReserveAddressState == RtuPartState.Normal
                ? "Available"
                : "Not available";
        }

        private Brush RtuAvalilabilityToBrush(bool isForeground)
        {
            var state = (int) MainAddressState + ReserveAddressState;
            if (state < 0)
                return Brushes.Red;
            if (state == 0)
                return Brushes.HotPink;
            return isForeground ? Brushes.Black : Brushes.Transparent;
        }
    }
}