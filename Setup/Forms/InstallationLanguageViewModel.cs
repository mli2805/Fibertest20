using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Setup
{
    public class InstallationLanguageViewModel : Screen
    {
        public List<string> Languages { get; set; }

        private string _selectedLanguage;
        public string SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                if (value == _selectedLanguage) return;
                _selectedLanguage = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsOkPressed;

        public InstallationLanguageViewModel()
        {
            Languages = new List<string>(){"English", "Русский"};
            SelectedLanguage = Languages.First();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Language_choice;
        }

        public void ButtonOk()
        {
            IsOkPressed = true;
            TryClose();
        }
    }
}
