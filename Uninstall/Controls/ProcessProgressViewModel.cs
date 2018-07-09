using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.UtilsLib;

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

        public ObservableCollection<string> ProgressLines { get; set; } = new ObservableCollection<string>();


        private string _fibertestFolder;
        private bool _isFullUninstall;
        public bool IsUninstallSuccessful { get; set; }
        private bool _isDone;

        public void RunUninstall(string fibertestFolder, bool isFullUninstall)
        {
            _fibertestFolder = fibertestFolder;
            _isFullUninstall = isFullUninstall;

            var bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.DoWork += Bw_DoWork;
            bw.ProgressChanged += Bw_ProgressChanged;
            bw.RunWorkerCompleted += Bw_RunWorkerCompleted;

            bw.RunWorkerAsync();
        }

        private void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsDone = true;
        }

        private void Bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var code = e.ProgressPercentage;
            var st = (string)e.UserState;
            if (code >= 10)
                ProgressLines.Add(((BwReturnProgressCode)code).GetLocalizedString(st));
        }

        private void Bw_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            IsUninstallSuccessful = new UninstallOperations().Do(worker, _fibertestFolder, _isFullUninstall);
        }

    }
}
