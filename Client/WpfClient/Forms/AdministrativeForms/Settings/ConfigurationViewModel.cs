using System.Collections.Generic;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class ConfigurationViewModel : Screen
    {
        private readonly IniFile _iniFile;
        private readonly GraphReadModel _graphReadModel;
        private readonly CurrentGis _currentGis;
        private readonly SoundManager _soundManager;
        public List<string> SupportedLanguages { get; set; } = new List<string>(){@"ru-RU", @"en-US"};
        public List<string> MapProviders { get; set; } = new List<string>(){@"OpenStreetMap", @"GoogleMap", @"YandexMap"};

        public bool IsEnabled { get; set; }

        private bool _isSoundOn;
        private string _soundButtonContent;
        public string SoundButtonContent
        {
            get => _soundButtonContent;
            set
            {
                if (value == _soundButtonContent) return;
                _soundButtonContent = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _isGraphVisibleOnStart;
        public bool IsGraphVisibleOnStart
        {
            get => _isGraphVisibleOnStart;
            set
            {
                _isGraphVisibleOnStart = value;
                _iniFile.Write(IniSection.Miscellaneous, IniKey.IsGraphVisibleOnStart, _isGraphVisibleOnStart);
            }
        }


        private string _selectedLanguage;
        public string SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                _selectedLanguage = value;
                _iniFile.Write(IniSection.General, IniKey.Culture, _selectedLanguage);
            }
        }

        private string _selectedProvider;
        public string SelectedProvider
        {
            get => _selectedProvider;
            set
            {
                _selectedProvider = value; 
                _iniFile.Write(IniSection.Map, IniKey.GMapProvider, _selectedProvider);
                if (!_currentGis.IsGisOn)
                    _graphReadModel.MainMap.MapProvider = GMapProviderExt.Get(_selectedProvider);
            }
        }

        public ConfigurationViewModel(IniFile iniFile, GraphReadModel graphReadModel, CurrentGis currentGis,
            CurrentUser currentUser, SoundManager soundManager)
        {
            _iniFile = iniFile;
            _graphReadModel = graphReadModel;
            _currentGis = currentGis;
            _soundManager = soundManager;
            IsEnabled = currentUser.Role < Role.Superclient;

            _isGraphVisibleOnStart = _iniFile.Read(IniSection.Miscellaneous, IniKey.IsGraphVisibleOnStart, false);
            SelectedLanguage = _iniFile.Read(IniSection.General, IniKey.Culture, @"ru-RU");
            _selectedProvider = _iniFile.Read(IniSection.Map, IniKey.GMapProvider, MapProviders[0]);
            SoundButtonContent = Resources.SID_Turn_alarm_on;
            _isSoundOn = false;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Client_settings;
        }    
        
        public void TestSound()
        {
            if (_isSoundOn) _soundManager.StopAlert(); else _soundManager.StartAlert();
            _isSoundOn = !_isSoundOn;
            SoundButtonContent = _isSoundOn ? Resources.SID_Turn_alarm_off : Resources.SID_Turn_alarm_on;
        }

        public void Close() { TryClose(); }
    }
}
