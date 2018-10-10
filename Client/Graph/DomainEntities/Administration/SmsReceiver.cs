using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
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
        public bool IsActivated { get; set; }
    }

    public static class SmsReceiverExt
    {
        public static bool ShouldUserReceiveMoniResult(this SmsReceiver smsReceiver, FiberState traceState)
        {
            switch (traceState)
            {
                case FiberState.NoFiber:
                case FiberState.FiberBreak:
                    return smsReceiver.IsFiberBreakOn;
                case FiberState.Critical:
                    return smsReceiver.IsCriticalOn;
                default: return false;
            }
        }
    }
}