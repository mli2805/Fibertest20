﻿using System.Collections.Generic;
using System.Windows;
using Caliburn.Micro;
using GMap.NET.MapProviders;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
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
                NotifyOfPropertyChange(nameof(GisModeMessage));
            }
        }

        public string GisModeMessage => IsInWithoutMapMode ? Resources.SID_In__Without_Map__mode : Resources.SID_Map_is_displayed;

        public bool IsRoot { get; set; }
        public Visibility SecondBoxVisibility { get; set; }

        private Visibility _thirdBoxVisibility;
        public Visibility ThirdBoxVisibility
        {
            get { return _thirdBoxVisibility; }
            set
            {
                if (value == _thirdBoxVisibility) return;
                _thirdBoxVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        public List<string> MapProviders { get; set; } = new List<string>() { @"OpenStreetMap", @"GoogleMap", @"YandexMap" };
        private string _selectedProvider;
        public string SelectedProvider
        {
            get => _selectedProvider;
            set
            {
                _selectedProvider = value;
                _iniFile.Write(IniSection.Map, IniKey.GMapProvider, _selectedProvider);
                if (_currentGis.IsGisOn)
                    _graphReadModel.MainMap.MapProvider = GMapProviderExt.Get(_selectedProvider);
            }
        }

        private bool _isMapTemporarilyVisibleInThisClient;

        public bool IsMapTemporarilyVisibleInThisClient
        {
            get => _isMapTemporarilyVisibleInThisClient;
            set
            {
                _isMapTemporarilyVisibleInThisClient = value;
                _currentGis.IsRootTempGisOn = value;
                _graphReadModel.MainMap.MapProvider = value 
                    ? GMapProviderExt.Get(_iniFile.Read(IniSection.Map, IniKey.GMapProvider, @"OpenStreetMap")) 
                    : GMapProviders.EmptyProvider;
                ThirdBoxVisibility = _currentGis.IsGisOn ? Visibility.Visible : Visibility.Collapsed;
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
            SecondBoxVisibility = currentUser.Role <= Role.Root ? Visibility.Visible : Visibility.Collapsed;
            _selectedProvider = _iniFile.Read(IniSection.Map, IniKey.GMapProvider, MapProviders[0]);
            ThirdBoxVisibility = currentGis.IsGisOn ? Visibility.Visible : Visibility.Collapsed;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Gis_settings;
            _isMapTemporarilyVisibleInThisClient = _currentGis.IsRootTempGisOn;
        }

        public async void ChangeMode()
        {
            bool res;
            using (new WaitCursor())
            {
                res = await _c2DWcfManager.SaveGisMode(!IsInWithoutMapMode);
            }

            if (res)
            {
                IsInWithoutMapMode = !IsInWithoutMapMode;
                _currentGis.IsWithoutMapMode = IsInWithoutMapMode;
                if (IsInWithoutMapMode && IsMapTemporarilyVisibleInThisClient)
                {
                    var provider = _iniFile.Read(IniSection.Map, IniKey.GMapProvider, @"OpenStreetMap");
                    _graphReadModel.MainMap.MapProvider = GMapProviderExt.Get(provider);
                }
                ThirdBoxVisibility = _currentGis.IsGisOn ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Failed_to_save_GIS_mode_);
                _windowManager.ShowDialogWithAssignedOwner(vm);
            }
        }
    }
}
