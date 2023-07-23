using System.Collections.Generic;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Install
{
    public class InstTypeChoiceViewModel : PropertyChangedBase
    {

        private Visibility _visibility = Visibility.Collapsed;
        public Visibility Visibility
        {
            get => _visibility;
            set
            {
                if (value == _visibility) return;
                _visibility = value;
                NotifyOfPropertyChange();
            }
        }


        public InstSettingsForServerViewModel InstSettingsForServerViewModel { get; set; }
        public InstSettingsForClientViewModel InstSettingsForClientViewModel { get; set; }
        public InstallationSettingsForSuperClientViewModel InstallationSettingsForSuperClientViewModel { get; set; }

        public HeaderViewModel HeaderViewModel { get; set; } = new HeaderViewModel();
        public string Text1 { get; set; }
        public List<string> InstTypes { get; set; }

        private string _selectedType;

        public string SelectedType
        {
            get => _selectedType;
            set
            {
                if (value == _selectedType) return;
                _selectedType = value;
                InstSettingsForServerViewModel.Visibility = _selectedType == "Data Center" ? Visibility.Visible : Visibility.Collapsed;
                InstSettingsForClientViewModel.Visibility = _selectedType == "Client" ? Visibility.Visible : Visibility.Collapsed;
                InstallationSettingsForSuperClientViewModel.Visibility =
                    _selectedType == "Super Client" && InstallationSettingsForSuperClientViewModel.CheckCurrentClientVersion()
                        ? Visibility.Visible : Visibility.Collapsed;
                NotifyOfPropertyChange();
            }
        }

        public InstTypeChoiceViewModel(CurrentInstallation currentInstallation)
        {
            HeaderViewModel.InBold = Resources.SID_Installation_Type;
            HeaderViewModel.Explanation = string.Format(Resources.SID_Please_select_the_type_of__0__install, currentInstallation.MainName);

            InstSettingsForServerViewModel = new InstSettingsForServerViewModel(currentInstallation);
            InstSettingsForClientViewModel = new InstSettingsForClientViewModel();
            InstallationSettingsForSuperClientViewModel = new InstallationSettingsForSuperClientViewModel(currentInstallation);

            Text1 = string.Format(Resources.SID_Select_the_type_of__0__install__Click_Next_to_continue_, currentInstallation.MainName);
            InstTypes = new List<string>() { "Client", "Data Center", "Super Client" };
            SelectedType = InstTypes[0];
        }

        public InstallationType GetSelectedType()
        {
            switch (SelectedType)
            {
                case "Data Center": return InstallationType.Datacenter;
                case "Client": return InstallationType.Client;
                case "Super Client": return InstallationType.SuperClient;
                default: return InstallationType.Client;
            }
        }

     
    }
}
