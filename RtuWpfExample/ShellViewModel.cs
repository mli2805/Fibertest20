using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.RtuWpfExample;
using Iit.Fibertest.Utils35;
using Iit.Fibertest.WpfCommonViews;
using Microsoft.Win32;

namespace RtuWpfExample
{
    public class ShellViewModel : PropertyChangedBase, IShell
    {
        public OtdrManager OtdrManager { get; set; }

        public bool ShouldForceLmax { get; set; } = true;

        private string _baseFileName;
        public string BaseFileName
        {
            get { return _baseFileName; }
            set
            {
                if (Equals(value, _baseFileName)) return;
                _baseFileName = value;
                NotifyOfPropertyChange(() => BaseFileName);
            }
        }

        private string _resultFileName;
        public string ResultFileName
        {
            get { return _resultFileName; }
            set
            {
                if (Equals(value, _resultFileName)) return;
                _resultFileName = value;
                NotifyOfPropertyChange(() => ResultFileName);
            }
        }

        private string _measFileName;
        public string MeasFileName
        {
            get { return _measFileName; }
            set
            {
                if (Equals(value, _measFileName)) return;
                _measFileName = value;
                NotifyOfPropertyChange(() => MeasFileName);
            }
        }

        private string _initializationMessage;
        public string InitializationMessage
        {
            get { return _initializationMessage; }
            set
            {
                if (Equals(value, _initializationMessage)) return;
                _initializationMessage = value;
                NotifyOfPropertyChange(() => InitializationMessage);
            }
        }

        private string _message;
        public string Message
        {
            get { return _message; }
            set
            {
                if (Equals(value, _message)) return;
                _message = value;
                NotifyOfPropertyChange(() => Message);
            }
        }


        private bool _isLibraryInitialized;
        public bool IsLibraryInitialized
        {
            get { return _isLibraryInitialized; }
            set
            {
                if (Equals(value, _isLibraryInitialized)) return;
                _isLibraryInitialized = value;
                NotifyOfPropertyChange(() => IsLibraryInitialized);
            }
        }

        private bool _isOtdrConnected;
        public bool IsOtdrConnected
        {
            get { return _isOtdrConnected; }
            set
            {
                if (Equals(value, _isOtdrConnected)) return;
                _isOtdrConnected = value;
                NotifyOfPropertyChange(() => IsOtdrConnected);
            }
        }

        private bool _isMeasurementInProgress;
        public bool IsMeasurementInProgress
        {
            get { return _isMeasurementInProgress; }
            set
            {
                if (Equals(value, _isMeasurementInProgress)) return;
                _isMeasurementInProgress = value;
                NotifyOfPropertyChange(() => IsMeasurementInProgress);
            }
        }
        
        public string IpAddress { get; set; }

        private readonly Logger35 _rtuLogger;

        public ShellViewModel()
        {
            _rtuLogger = new Logger35();
            _rtuLogger.AssignFile("rtu.log");

            IpAddress = "192.168.96.53";
//            IpAddress = "192.168.96.52";
            //            IpAddress = "172.16.4.10";
            //IpAddress = "192.168.88.101";

            BaseFileName = @"c:\temp\base3ev.sor";
            MeasFileName = @"c:\temp\123.sor";
            ResultFileName = @"c:\temp\measwithbase.sor";


            OtdrManager = new OtdrManager(@"OtdrMeasEngine\", _rtuLogger);
            var initializationResult = OtdrManager.LoadDll();
            if (initializationResult != "")
                InitializationMessage = initializationResult;
            IsLibraryInitialized = OtdrManager.InitializeLibrary();
        }

        public async Task ConnectOtdr()
        {
            InitializationMessage = "Wait, please...";

            await ConnectionProcess();

            InitializationMessage = IsOtdrConnected ? "OTDR connected successfully!" : "OTDR connection failed!";
        }

        private async Task ConnectionProcess()
        {
            using (new WaitCursor())
            {
                await Task.Run(() => OtdrManager.ConnectOtdr(IpAddress));
                IsOtdrConnected = OtdrManager.IsOtdrConnected;
            }
        }

        public void OtauView()
        {
            var vm = new OtauViewModel(IpAddress, _rtuLogger);
            IWindowManager windowManager = new WindowManager();
            windowManager.ShowWindow(vm);
        }

        public void LaunchOtdrParamView()
        {
            var vm = new OtdrParamViewModel(OtdrManager.IitOtdr);
            IWindowManager windowManager = new WindowManager();
            windowManager.ShowDialog(vm);
        }

        public async Task StartMeasurement()
        {
            using (new WaitCursor())
            {
                IsMeasurementInProgress = true;
                Message = "Wait, please...";

                await Task.Run(() => OtdrManager.DoManualMeasurement(ShouldForceLmax));

                IsMeasurementInProgress = false;
                Message = "Measurement is finished.";

                var lastSorDataBuffer = OtdrManager.GetLastSorDataBuffer();
                if (lastSorDataBuffer == null)
                    return;
                var sorData = OtdrManager.ApplyFilter(OtdrManager.ApplyAutoAnalysis(lastSorDataBuffer), false);
                sorData.Save(MeasFileName);
            }
        }

        public void ChooseBaseFilename()
        {
            var fd = new OpenFileDialog();
            fd.Filter = "Sor files (*.sor)|*.sor";
            fd.InitialDirectory = @"c:\temp\";
            if (fd.ShowDialog() == true)
                BaseFileName = fd.FileName;
        }

        public void ChooseResultFilename()
        {
            var fd = new OpenFileDialog();
            fd.Filter = "Sor files (*.sor)|*.sor";
            fd.InitialDirectory = @"c:\temp\";
            if (fd.ShowDialog() == true)
                ResultFileName = fd.FileName;
        }

        public void ChooseMeasFilename()
        {
            var fd = new SaveFileDialog();
            fd.Filter = "Sor files (*.sor)|*.sor";
            fd.InitialDirectory = @"c:\temp\";
            if (fd.ShowDialog() == true)
                MeasFileName = fd.FileName;
        }

        public async Task StartMeasurementWithBase()
        {
            using (new WaitCursor())
            {
                IsMeasurementInProgress = true;
                Message = "Wait, please...";

                byte[] buffer = File.ReadAllBytes(BaseFileName);
                await Task.Run(() => OtdrManager.MeasureWithBase(buffer));

                IsMeasurementInProgress = false;
                Message = "Measurement is finished.";

                var lastSorDataBuffer = OtdrManager.GetLastSorDataBuffer();
                if (lastSorDataBuffer == null)
                    return;
                var sorData = OtdrManager.ApplyFilter(OtdrManager.ApplyAutoAnalysis(lastSorDataBuffer), OtdrManager.IsFilterOnInBase(buffer));
                sorData.Save(MeasFileName);
            }
        }

        // button
        public void CompareMeasurementWithBase()
        {
            var bufferBase = File.ReadAllBytes(BaseFileName);
            var bufferMeas = File.ReadAllBytes(MeasFileName);

            var moniResult = OtdrManager.CompareMeasureWithBase(bufferBase, bufferMeas, true);
            _rtuLogger.AppendLine($"Comparison end. IsFiberBreak = {moniResult.IsFiberBreak}, IsNoFiber = {moniResult.IsNoFiber}");
        }


        private bool _isMonitoringCycleCanceled;
        private object _cycleLockOb = new object();
        public async Task StartCycle()
        {
            lock (_cycleLockOb)
            {
                _isMonitoringCycleCanceled = false;
            }

            int c = 0;
            byte[] baseBytes = File.ReadAllBytes(BaseFileName);
//            var isFilterOn = OtdrManager.IsFilterOnInBase(baseBytes);

            while (true)
            {
                lock (_cycleLockOb)
                {
                    if (_isMonitoringCycleCanceled)
                    {
                        OtdrManager.InterruptMeasurement();
                        break;
                    }
                }

                using (new WaitCursor())
                {
                    IsMeasurementInProgress = true;
                    Message = $"Monitoring cycle {c}. Wait, please...";
                    _rtuLogger.AppendLine($"Monitoring cycle {c}.");

                    await Task.Run(() => OtdrManager.MeasureWithBase(baseBytes));

                    IsMeasurementInProgress = false;
                    Message = $"{c}th measurement is finished.";

                    var moniResult = OtdrManager.CompareMeasureWithBase(baseBytes,
                        OtdrManager.ApplyAutoAnalysis(OtdrManager.GetLastSorDataBuffer()), true); // is ApplyAutoAnalysis necessary ?
                    _rtuLogger.AppendLine(moniResult.Result.ToString());
                }
                c++;
            }
        }

        public void StopCycle()
        {
            lock (_cycleLockOb)
            {
                _isMonitoringCycleCanceled = true;
                InterruptMeasurement();
            }
        }


        public void InterruptMeasurement()
        {
            OtdrManager.InterruptMeasurement();
            Message = "Stop command is sent";
        }

        public void ShowRftsEvents()
        {
            var buffer = File.ReadAllBytes(ResultFileName);
            var sorData = SorData.FromBytes(buffer);
            if (sorData.RftsEvents.MonitoringResult == (int) ComparisonReturns.NoLink)
            {
                MessageBox.Show("No Fiber!", "Events");
                return;
            }
            var vm = new RftsEventsViewModel(sorData);
            IWindowManager windowManager = new WindowManager();
            windowManager.ShowDialog(vm);
        }

        public void ShowTraceState() { }
    }
}