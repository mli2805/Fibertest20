using System.Collections.ObjectModel;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;

namespace Setup
{
    public class ProcessProgressViewModel : PropertyChangedBase
    {
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

        public ProcessProgressViewModel()
        {
            HeaderViewModel.InBold = Resources.SID_Installing;
            HeaderViewModel.Explanation = Resources.SID_Please_wait_while_IIT_Fibertest_2_0_is_being_installed_;

        }

        public void SayGoodbye()
        {
            HeaderViewModel.InBold = Resources.SID_Installation_Complete;
            HeaderViewModel.Explanation = Resources.SID_Setup_was_completed_successfully;
        }
    }
}
