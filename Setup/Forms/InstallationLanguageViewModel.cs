using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;

namespace Setup
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

        public InstallationLanguageViewModel()
        {
            Languages = new List<string>(){"English", "Русский"};
            SelectedLanguage = Languages.First();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Choose language";
        }

        public void ButtonOk()
        {
            TryClose();
        }
    }
}
