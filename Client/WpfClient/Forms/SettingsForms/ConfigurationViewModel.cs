using System;
using System.Collections.Generic;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class ConfigurationViewModel : Screen
    {
        private readonly IniFile _iniFile;
        private readonly CurrentClientConfiguration _currentClientConfiguration;
        private readonly SoundManager _soundManager;
        public List<string> SupportedLanguages { get; set; } = new List<string>(){@"ru-RU", @"en-US"};

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

        private bool _doNotSignalAboutSuspicion;
        public bool DoNotSignalAboutSuspicion
        {
            get => _doNotSignalAboutSuspicion;
            set
            {
                if (value == _doNotSignalAboutSuspicion) return;
                _doNotSignalAboutSuspicion = value;
                NotifyOfPropertyChange();
                _iniFile.Write(IniSection.Miscellaneous, IniKey.DoNotSignalAboutSuspicion, _doNotSignalAboutSuspicion);
                _currentClientConfiguration.DoNotSignalAboutSuspicion = _doNotSignalAboutSuspicion;
            }
        }

        public ConfigurationViewModel(IniFile iniFile, CurrentClientConfiguration currentClientConfiguration, SoundManager soundManager)
        {
            _iniFile = iniFile;
            _currentClientConfiguration = currentClientConfiguration;
            _soundManager = soundManager;

            SelectedLanguage = _iniFile.Read(IniSection.General, IniKey.Culture, @"ru-RU");
            DoNotSignalAboutSuspicion =
                _iniFile.Read(IniSection.Miscellaneous, IniKey.DoNotSignalAboutSuspicion, false);
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

        public override void CanClose(Action<bool> callback)
        {
            if (_isSoundOn)
                _soundManager.StopAlert();
            callback(true);
        }

        public void Close() { TryClose(); }
    }
}
