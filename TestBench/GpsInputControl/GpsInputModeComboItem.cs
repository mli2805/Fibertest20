using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.TestBench
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
                    return Resources.SID___ddd_dddddd;
                case GpsInputMode.DegreesAndMinutes:
                    return Resources.SID___ddd_mm_mmmm;
                case GpsInputMode.DegreesMinutesAndSeconds:
                    return Resources.SID___ddd_mm_ss_ss;
                default:
                    return "";
            }
        }
    }
}