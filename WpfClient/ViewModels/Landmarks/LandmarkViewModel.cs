using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class LandmarkViewModel : Screen
    {
        private readonly ReadModel _readModel;
        public string NodeTitle { get; set; }
        public string NodeComment { get; set; }
        public string EquipmentTitle { get; set; }
        public EquipmentType EquipmentType { get; set; }
        public GpsInputViewModel GpsInputViewModel { get; set; }
        public string RtuTitle { get; set; }
        public string TraceTitle { get; set; }
        public string Location { get; set; }
        public int LandmarkNumber { get; set; }
        public string EventNumber { get; set; }

        public LandmarkViewModel(ReadModel readModel)
        {
            _readModel = readModel;
        }

        public void Initialize(Landmark landmark)
        {
            DisplayName = string.Format(Resources.SID_Landmark___0__, landmark.EquipmentTitle);
            NodeTitle = landmark.NodeTitle;
            EquipmentTitle = landmark.EquipmentTitle;

            GpsInputViewModel = new GpsInputViewModel(GpsInputMode.DegreesMinutesAndSeconds, landmark.GpsCoors);
        }
    }
}
