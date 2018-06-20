using System.Collections.Generic;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Setup
{
    public class InstTypeChoiceViewModel : PropertyChangedBase
    {
        private Visibility _visibility = Visibility.Collapsed;
        private string _selectedType;

        public Visibility Visibility
        {
            get { return _visibility; }
            set
            {
                if (value == _visibility) return;
                _visibility = value;
                NotifyOfPropertyChange();
            }
        }

        public HeaderViewModel HeaderViewModel { get; set; } = new HeaderViewModel();
        public string Text1 { get; set; }
        public string Text2 { get; set; } = Resources.SID_Type_of_install_;
        public List<string> InstTypes { get; set; }

        public string SelectedType
        {
            get => _selectedType;
            set
            {
                if (value == _selectedType) return;
                _selectedType = value;
                NotifyOfPropertyChange();
            }
        }

        public InstTypeChoiceViewModel(CurrentInstallation currentInstallation)
        {
            HeaderViewModel.InBold = Resources.SID_Installation_Type;
            HeaderViewModel.Explanation = string.Format(Resources.SID_Please_select_the_type_of__0__install, currentInstallation.MainName);

            Text1 = string.Format(Resources.SID_Select_the_type_of__0__install__Click_Next_to_continue_, currentInstallation.MainName);
            InstTypes = new List<string>() { "Client", "Data Center", "RTU Manager" };
            SelectedType = InstTypes[1];
        }

        public InstallationType GetSelectedType()
        {
            switch (SelectedType)
            {
                case "Client": return InstallationType.Client;
                case "Data Center": return InstallationType.Datacenter;
                case "RTU Manager": return InstallationType.RtuManager;
                default: return InstallationType.Client;
            }

        }
    }
}
