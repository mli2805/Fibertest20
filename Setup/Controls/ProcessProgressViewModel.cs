using System.Collections.ObjectModel;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;

namespace Setup
{
    public class ProcessProgressViewModel : PropertyChangedBase
    {
        private readonly SetupOperations _setupOperations;
        private Visibility _visibility = Visibility.Collapsed;

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

        public ObservableCollection<string> ProgressLines { get; set; } = new ObservableCollection<string>();

        public string Text1 { get; set; }

        public ProcessProgressViewModel(CurrentInstallation currentInstallation, SetupOperations setupOperations)
        {
            _setupOperations = setupOperations;
            HeaderViewModel.InBold = Resources.SID_Installing;
            HeaderViewModel.Explanation = string.Format(Resources.SID_Please_wait_while__0__is_being_installed,
                currentInstallation.MainName);
        }

        public void RunSetup()
        {
            _setupOperations.Run(ProgressLines);
        }

        public void SayGoodbye()
        {
            HeaderViewModel.InBold = Resources.SID_Installation_Complete;
            HeaderViewModel.Explanation = Resources.SID_Setup_was_completed_successfully;
        }
    }
}