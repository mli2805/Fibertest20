using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class GisSettingsViewModel : Screen
    {
        public CurrentDatacenterParameters CurrentDatacenterParameters { get; }
        public bool IsRoot { get; set; }

        public GisSettingsViewModel(CurrentDatacenterParameters currentDatacenterParameters, CurrentUser currentUser)
        {
            CurrentDatacenterParameters = currentDatacenterParameters;
            IsRoot = currentUser.Role <= Role.Root;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Gis_settings;
        }

        public void Save()
        {
            TryClose();
        }
    }
}
