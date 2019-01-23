using Caliburn.Micro;
using GMap.NET.MapProviders;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class GisSettingsViewModel : Screen
    {
        private readonly CurrentDatacenterParameters _currentDatacenterParameters;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly IWindowManager _windowManager;
        private readonly IniFile _iniFile;
        private readonly GraphReadModel _graphReadModel;

        private bool _isInWithoutMapMode;
        public bool IsInWithoutMapMode
        {
            get => _isInWithoutMapMode;
            set
            {
                if (value == _isInWithoutMapMode) return;
                _isInWithoutMapMode = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(IsSecondCheckBoxEnabled));
            }
        }

        public bool IsRoot { get; set; }

        public bool IsSecondCheckBoxEnabled => IsInWithoutMapMode && IsRoot;

        private bool _isMapTemporarilyVisibleInThisClient;
        public bool IsMapTemporarilyVisibleInThisClient
        {
            get => _isMapTemporarilyVisibleInThisClient;
            set
            {
                _isMapTemporarilyVisibleInThisClient = value;
                if (value)
                {
                    var provider = _iniFile.Read(IniSection.Map, IniKey.GMapProvider, @"OpenStreetMap");
                    _graphReadModel.MainMap.MapProvider = GMapProviderExt.Get(provider);
                }
                else
                {
                    _graphReadModel.MainMap.MapProvider = GMapProviders.EmptyProvider;
                }
            }
        }

        public GisSettingsViewModel(CurrentDatacenterParameters currentDatacenterParameters, CurrentUser currentUser,
            IWcfServiceForClient c2DWcfManager, IWindowManager windowManager,
            IniFile iniFile, GraphReadModel graphReadModel)
        {
            _currentDatacenterParameters = currentDatacenterParameters;
            IsInWithoutMapMode = !currentDatacenterParameters.IsInGisVisibleMode;
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;
            _iniFile = iniFile;
            _graphReadModel = graphReadModel;
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
            {
                if (IsInWithoutMapMode && IsMapTemporarilyVisibleInThisClient)
                {
                    var provider = _iniFile.Read(IniSection.Map, IniKey.GMapProvider, @"OpenStreetMap");
                    _graphReadModel.MainMap.MapProvider = GMapProviderExt.Get(provider);
                }
                TryClose();
            }
            else
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Failed_to_save_GIS_mode_);
                _windowManager.ShowDialogWithAssignedOwner(vm);
            }
        }
    }
}
