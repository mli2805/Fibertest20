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
                    return @"±ddd.dddddd°";
                case GpsInputMode.DegreesAndMinutes:
                    return @"±ddd°mm.mmmm′";
                case GpsInputMode.DegreesMinutesAndSeconds:
                    return @"±ddd°mm′ss.ss″";
                default:
                    return "";
            }
        }
    }
}