using System.Collections.Generic;
using Caliburn.Micro;
using GMap.NET.MapProviders;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Client
{
    public class ConfigurationViewModel : Screen
    {
        private readonly IniFile _iniFile;
        private readonly CurrentDatacenterParameters _currentDatacenterParameters;
        private readonly GraphReadModel _graphReadModel;
        private bool _isGraphVisibleOnStart;
        private string _selectedLanguage;
        private string _selectedProvider;
        public List<string> SupportedLanguages { get; set; } = new List<string>(){@"ru-RU", @"en-US"};
        public List<string> MapProviders { get; set; } = new List<string>(){@"OpenStreetMap", @"GoogleMap", @"YandexMap"};

        public bool IsEnabled { get; set; }

        public ConfigurationViewModel(IniFile iniFile, GraphReadModel graphReadModel, 
            CurrentUser currentUser, CurrentDatacenterParameters currentDatacenterParameters)
        {
            _iniFile = iniFile;
            _currentDatacenterParameters = currentDatacenterParameters;
            _graphReadModel = graphReadModel;
            IsEnabled = currentUser.Role < Role.Superclient;

            _isGraphVisibleOnStart = _iniFile.Read(IniSection.Miscellaneous, IniKey.IsGraphVisibleOnStart, false);
            SelectedLanguage = _iniFile.Read(IniSection.General, IniKey.Culture, @"ru-RU");
            _selectedProvider = _iniFile.Read(IniSection.Map, IniKey.GMapProvider, MapProviders[0]);
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Client_settings;
        }

        public bool IsGraphVisibleOnStart
        {
            get => _isGraphVisibleOnStart;
            set
            {
                _isGraphVisibleOnStart = value;
                _iniFile.Write(IniSection.Miscellaneous, IniKey.IsGraphVisibleOnStart, _isGraphVisibleOnStart);
            }
        }


        public string SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                _selectedLanguage = value;
                _iniFile.Write(IniSection.General, IniKey.Culture, _selectedLanguage);
            }
        }

        public string SelectedProvider
        {
            get => _selectedProvider;
            set
            {
                _selectedProvider = value; 
                _iniFile.Write(IniSection.Map, IniKey.GMapProvider, _selectedProvider);
                if (_currentDatacenterParameters.IsInGisVisibleMode)
                    _graphReadModel.MainMap.MapProvider = GMapProviderExt.Get(_selectedProvider);
            }
        }

        public void Close() { TryClose(); }
    }

    public static class GMapProviderExt
    {
        public static GMapProvider Get(string providerName)
        {
            switch (providerName)
            {
                case "OpenStreetMap":
                {
                    return GMapProviders.OpenStreetMap;
                }
                case "GoogleMap":
                {
                    return GMapProviders.GoogleMap;
                }
                case "YandexMap":
                {
                    return GMapProviders.YandexMap;
                }
                default:
                    return GMapProviders.EmptyProvider;
            }
        }
    }
}
