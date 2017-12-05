using System.Collections.Generic;
using System.Windows.Media;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;

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
        public Brush ReserveAddressBrush => ReserveAddressState.GetBrush(true);
        public string ReserveAddress { get; set; }
        public string ReserveAddressStateOnScreen => ReserveAddressState.ToLocalizedString();

        public int FullPortCount { get; set; }
        public int OwnPortCount { get; set; }
        public string PortCountOnScreen => $@"{OwnPortCount}/{FullPortCount}";

        public int TraceCount { get; set; }
        public int BopCount { get; set; }

        public RtuPartState BopState { get; set; }
        public string BopStateOnScreen => BopState.ToLocalizedString();
        public Brush BopStateBrush => BopState.GetBrush(true);

        public FiberState TracesState { get; set; }
        public string TracesStateOnScreen => TracesState.ToLocalizedString();
        public Brush TracesStateBrush => TracesState.GetBrush(true);
        public string MonitoringMode { get; set; }

        public string CurrentMeasurementStep { get; set; }

        public List<PortLineVm> Ports { get; set; }

        //------------------
        private string RtuAvailabilityToString()
        {
            return MainAddressState == RtuPartState.Normal || ReserveAddressState == RtuPartState.Normal
                ? Resources.SID_Available
                : Resources.SID_Not_available;
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