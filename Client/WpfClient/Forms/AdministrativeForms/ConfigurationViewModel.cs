using System.Collections.Generic;
using Caliburn.Micro;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Client
{
    public class ConfigurationViewModel : Screen
    {
        private readonly IniFile _iniFile;
        private bool _isGraphVisibleOnStart;
        private string _selectedLanguage;

        public ConfigurationViewModel(IniFile iniFile)
        {
            _iniFile = iniFile;

            _isGraphVisibleOnStart = _iniFile.Read(IniSection.Miscellaneous, IniKey.IsGraphVisibleOnStart, true);
            SelectedLanguage = SupportedLanguages[0];
        }

        public bool IsGraphVisibleOnStart
        {
            get
            {
                return _isGraphVisibleOnStart; 
            }
            set
            {
                _isGraphVisibleOnStart = value;
                _iniFile.Write(IniSection.Miscellaneous, IniKey.IsGraphVisibleOnStart, _isGraphVisibleOnStart);
            }
        }

        public List<string> SupportedLanguages { get; set; } = new List<string>(){@"ru-RU", @"en-US"};

        public string SelectedLanguage
        {
            get { return _selectedLanguage; }
            set
            {
                _selectedLanguage = value;
                _iniFile.Write(IniSection.General, IniKey.Culture, _selectedLanguage);
            }
        }
    }
}
