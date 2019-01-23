using System.Windows;
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
        private readonly CurrentGis _currentGis;
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
                NotifyOfPropertyChange(nameof(SecondBoxVisibility));
            }
        }

        public bool IsRoot { get; set; }

        public Visibility SecondBoxVisibility =>
           IsInWithoutMapMode && IsRoot ? Visibility.Visible : Visibility.Collapsed;

        private bool _isMapTemporarilyVisibleInThisClient;
        public bool IsMapTemporarilyVisibleInThisClient
        {
            get => _isMapTemporarilyVisibleInThisClient;
            set
            {
                _isMapTemporarilyVisibleInThisClient = value;
                _currentGis.IsRootTempGisOn = value;
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

        public GisSettingsViewModel(CurrentUser currentUser, CurrentGis currentGis,
            IWcfServiceForClient c2DWcfManager, IWindowManager windowManager,
            IniFile iniFile, GraphReadModel graphReadModel)
        {
            _currentGis = currentGis;
            IsInWithoutMapMode = currentGis.IsWithoutMapMode;
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;
            _iniFile = iniFile;
            _graphReadModel = graphReadModel;
            IsRoot = currentUser.Role <= Role.Root;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Gis_settings;
            _isMapTemporarilyVisibleInThisClient = _currentGis.IsRootTempGisOn;
        }

        public async void Apply()
        {
            bool res;
            using (new WaitCursor())
            {
                _currentGis.IsWithoutMapMode = IsInWithoutMapMode;
                res = await _c2DWcfManager.SaveGisMode(IsInWithoutMapMode);
            }

            if (res)
            {
                if (IsInWithoutMapMode && IsMapTemporarilyVisibleInThisClient)
                {
                    var provider = _iniFile.Read(IniSection.Map, IniKey.GMapProvider, @"OpenStreetMap");
                    _graphReadModel.MainMap.MapProvider = GMapProviderExt.Get(provider);
                }
            }
            else
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Failed_to_save_GIS_mode_);
                _windowManager.ShowDialogWithAssignedOwner(vm);
            }
        }
    }
}
