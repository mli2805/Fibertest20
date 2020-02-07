using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.InstallRtu
{
    public class ProcessProgressViewModel : PropertyChangedBase
    {
        private readonly CurrentRtuInstallation _currentRtuInstallation;
        private readonly RtuInstallManager _rtuInstallManager;
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

        public bool IsDone
        {
            get { return _isDone; }
            set
            {
                if (value == _isDone) return;
                _isDone = value;
                NotifyOfPropertyChange();
            }
        }

        public HeaderViewModel HeaderViewModel { get; set; } = new HeaderViewModel();

        public ObservableCollection<string> ProgressLines { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> FileLines { get; set; } = new ObservableCollection<string>();

        public ProcessProgressViewModel(CurrentRtuInstallation currentRtuInstallation, RtuInstallManager rtuInstallManager)
        {
            _currentRtuInstallation = currentRtuInstallation;
            _rtuInstallManager = rtuInstallManager;
            HeaderViewModel.InBold = Resources.SID_Installing;
            HeaderViewModel.Explanation = string.Format(Resources.SID_Please_wait_while__0__is_being_installed,
                _currentRtuInstallation.MainName);
        }

        private bool _setupResult;
        private bool _isDone;

        public void RunSetup()
        {
            var bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.DoWork += Bw_DoWork;
            bw.ProgressChanged += Bw_ProgressChanged;
            bw.RunWorkerCompleted += Bw_RunWorkerCompleted;

            bw.RunWorkerAsync();
        }

        private void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (_setupResult)
                SaySuccess();
            else SayFail();
            IsDone = true;
        }

        private void Bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var code = e.ProgressPercentage;
            var st = (string) e.UserState;
            if (code >= 10)
                ProgressLines.Add(((BwReturnProgressCode)code).GetLocalizedString(st));
            else FileLines.Add(st);
        }

        private void Bw_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            _setupResult = _rtuInstallManager.Run(worker);
        }

        private void SaySuccess()
        {
            HeaderViewModel.InBold = Resources.SID_Installation_Complete;
            HeaderViewModel.Explanation = Resources.SID_Setup_was_completed_successfully;
        }

      
        private void SayFail()
        {
            HeaderViewModel.InBold = Resources.SID_Installation_failed;
            HeaderViewModel.Explanation = string.Format(Resources.SID__0__installation_failed_, _currentRtuInstallation.MainName);
        }
    }
}