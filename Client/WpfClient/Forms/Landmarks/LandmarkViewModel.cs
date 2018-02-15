using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class LandmarkViewModel : Screen
    {
        private readonly ILifetimeScope _globalScope;
        public string NodeTitle { get; set; }
        public string NodeComment { get; set; }
        public string EquipmentTitle { get; set; }
        public EquipmentType EquipmentType { get; set; }
        public GpsInputViewModel GpsInputViewModel { get; set; }
        public string RtuTitle { get; set; }
        public string TraceTitle { get; set; }
        public string Location { get; set; }
        public string LandmarkNumber { get; set; }
        public string EventNumber { get; set; }

        public LandmarkViewModel(ILifetimeScope globalScope)
        {
            _globalScope = globalScope;
        }

        public void Initialize(Landmark landmark)
        {
            DisplayName = string.Format(Resources.SID_Landmark___0__, landmark.EquipmentTitle);
            NodeTitle = landmark.NodeTitle;
            EquipmentTitle = landmark.EquipmentTitle;
            Location = string.Format(Resources.SID__0__0_00000__km, landmark.Location);
            LandmarkNumber = string.Format(Resources.SID_Landmark___0_,landmark.Number);
            EventNumber = landmark.EventNumber == 0 ? 
                string.Format(Resources.SID_Event_N_0_, Resources.SID_no) :
                string.Format(Resources.SID_Event_N_0_, landmark.EventNumber);

            GpsInputViewModel = _globalScope.Resolve<GpsInputViewModel>();
            GpsInputViewModel.Initialize(landmark.GpsCoors);
        }
    }
}
