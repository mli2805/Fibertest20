using System.Collections.ObjectModel;
using System.Windows;
using Caliburn.Micro;

namespace Iit.Fibertest.Uninstall
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

        public ObservableCollection<string> ProgressLines { get; set; } = new ObservableCollection<string>();


        public void RunUninstall(string fibertestFolder)
        {
            new UninstallOperations().Do(ProgressLines, fibertestFolder);
        }
    }
}
