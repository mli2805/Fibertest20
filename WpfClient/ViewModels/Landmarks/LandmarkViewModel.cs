using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class LandmarkViewModel : Screen
    {
        public string NodeTitle { get; set; }
        public string EquipmentTitle { get; set; }
        public GpsInputViewModel GpsInputViewModel { get; set; }

        public LandmarkViewModel()
        {
        }

        public void Initialize(Landmark landmark)
        {
            DisplayName = $"Landmark {landmark.EquipmentTitle}";
            NodeTitle = landmark.NodeTitle;
            EquipmentTitle = landmark.EquipmentTitle;

            GpsInputViewModel = new GpsInputViewModel(GpsInputMode.DegreesMinutesAndSeconds, landmark.GpsCoors);
        }
    }
}
