using System;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class SmsReceiver
    {
        public string PhoneNumber { get; set; }
        public bool IsFiberBreakOn { get; set; }
        public bool IsCriticalOn { get; set; }
        public bool IsMajorOn { get; set; }
        public bool IsMinorOn { get; set; }
        public bool IsOkOn { get; set; }
        public bool IsNetworkEventsOn { get; set; }
        public bool IsBopEventsOn { get; set; }
        public bool IsRtuStatusEventsOn { get; set; }
        public bool IsActivated { get; set; }
    }
}