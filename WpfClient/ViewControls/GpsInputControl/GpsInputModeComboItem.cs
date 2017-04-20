using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class GpsInputModeComboItem
    {
        public GpsInputMode Mode { get; set; }

        public GpsInputModeComboItem(GpsInputMode mode)
        {
            Mode = mode;
        }

        public override string ToString()
        {
            switch (Mode)
            {
                case GpsInputMode.Degrees:
                    return Resources.SID_In_degrees;
                case GpsInputMode.DegreesAndMinutes:
                    return Resources.SID_In_degrees_and_minutes;
                case GpsInputMode.DegreesMinutesAndSeconds:
                    return Resources.SID_In_degrees_minutes_and_seconds;
                default:
                    return "";
            }
        }
    }
}