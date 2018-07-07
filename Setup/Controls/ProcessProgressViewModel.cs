using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Setup
{
    public class ProcessProgressViewModel : PropertyChangedBase
    {
        private readonly CurrentInstallation _currentInstallation;
        private readonly SetupManager _setupManager;
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

        public string Text1 { get; set; }

        public string CopiedFile
        {
            get { return _copiedFile; }
            set
            {
                if (value == _copiedFile) return;
                _copiedFile = value;
                NotifyOfPropertyChange();
            }
        }

        public ProcessProgressViewModel(CurrentInstallation currentInstallation, SetupManager setupManager)
        {
            _currentInstallation = currentInstallation;
            _setupManager = setupManager;
            HeaderViewModel.InBold = Resources.SID_Installing;
            HeaderViewModel.Explanation = string.Format(Resources.SID_Please_wait_while__0__is_being_installed,
                _currentInstallation.MainName);
        }

        private bool _setupResult;
        private bool _isDone;
        private string _copiedFile;

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
            var st = (string) e.UserState;
            var d = e.ProgressPercentage;
            if (d == 0)
                ProgressLines.Add(st);
            else CopiedFile = st;
        }

        private void Bw_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            _setupResult = _setupManager.Run(worker, CultureInfo.CurrentUICulture);
        }

        private void SaySuccess()
        {
            HeaderViewModel.InBold = Resources.SID_Installation_Complete;
            HeaderViewModel.Explanation = Resources.SID_Setup_was_completed_successfully;
        }

      
        private void SayFail()
        {
            HeaderViewModel.InBold = Resources.SID_Installation_failed;
            HeaderViewModel.Explanation = string.Format(Resources.SID__0__installation_failed_, _currentInstallation.MainName);
        }
    }
}