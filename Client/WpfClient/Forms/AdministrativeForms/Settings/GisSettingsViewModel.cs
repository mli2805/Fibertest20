using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfServiceForClientInterface;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class GisSettingsViewModel : Screen
    {
        private readonly CurrentDatacenterParameters _currentDatacenterParameters;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly IWindowManager _windowManager;
        public bool IsInWithoutMapMode {get; set; }
        public bool IsRoot { get; set; }

        public GisSettingsViewModel(CurrentDatacenterParameters currentDatacenterParameters, CurrentUser currentUser,
            IWcfServiceForClient c2DWcfManager, IWindowManager windowManager)
        {
            _currentDatacenterParameters = currentDatacenterParameters;
            IsInWithoutMapMode = !currentDatacenterParameters.IsInGisVisibleMode;
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;
            IsRoot = currentUser.Role <= Role.Root;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Gis_settings;
        }

        public async void Save()
        {
            bool res;
            using (new WaitCursor())
            {
                _currentDatacenterParameters.IsInGisVisibleMode = !IsInWithoutMapMode;
                res = await _c2DWcfManager.SaveGisMode(!IsInWithoutMapMode);
            }

            if (res)
                TryClose();
            else
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Failed_to_save_GIS_mode_);
                _windowManager.ShowDialogWithAssignedOwner(vm);
            }
        }
    }
}
