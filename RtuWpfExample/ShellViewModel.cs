using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.RtuWpfExample;
using Iit.Fibertest.Utils35;
using Iit.Fibertest.WpfCommonViews;
using Microsoft.Win32;
using Iit.Fibertest.DirectCharonLibrary;
using Iit.Fibertest.StringResources;

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
        private static IniFile _iniFile35;

        public ShellViewModel()
        {
            _rtuLogger = new Logger35();
            _rtuLogger.AssignFile("rtu.log");

            _iniFile35 = new IniFile();
            _iniFile35.AssignFile("rtu.ini");

            IpAddress = _iniFile35.Read(IniSection.General, IniKey.OtauIp, "192.168.96.53");

            BaseFileName = @"c:\temp\base3ev.sor";
            MeasFileName = @"c:\temp\123.sor";
            ResultFileName = @"c:\temp\measwithbase.sor";


            OtdrManager = new OtdrManager(@"OtdrMeasEngine\", _iniFile35, _rtuLogger);
            var initializationResult = OtdrManager.LoadDll();
            if (initializationResult != "")
                InitializationMessage = initializationResult;
            IsLibraryInitialized = OtdrManager.InitializeLibrary();
        }

        public async Task ConnectOtdr()
        {
//            InitializationMessage = "Wait, please...";
            InitializationMessage = Resources.SID_Wait__please___;

            await ConnectionProcess();

            InitializationMessage = IsOtdrConnected ? "OTDR connected successfully!" : "OTDR connection failed!";
        }

        private async Task ConnectionProcess() // button
        {
            using (new WaitCursor())
            {
                await Task.Run(() => OtdrManager.ConnectOtdr(IpAddress));
                IsOtdrConnected = OtdrManager.IsOtdrConnected;
            }
        }

        public void OtauView()
        {
            var otauPort = _iniFile35.Read(IniSection.General, IniKey.OtauPort, 23);
            var vm = new OtauViewModel(IpAddress, otauPort, _iniFile35, _rtuLogger);
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

                await Task.Run(() => OtdrManager.DoManualMeasurement(ShouldForceLmax, GetActiveChildCharon()));

                IsMeasurementInProgress = false;
                Message = "Measurement is finished.";

                var lastSorDataBuffer = OtdrManager.GetLastSorDataBuffer();
                if (lastSorDataBuffer == null)
                    return;
                var sorData = OtdrManager.ApplyFilter(OtdrManager.ApplyAutoAnalysis(lastSorDataBuffer), false);
                sorData.Save(MeasFileName);
            }
        }

        private Charon GetActiveChildCharon()
        {
            // Maybe it should be done in outer scope (something like RtuManager, which has its own MainCharon) ?
            var mainCharon = new Charon(new NetAddress(IpAddress, 23), _iniFile35, _rtuLogger);
            mainCharon.Initialize();
            NetAddress activeCharonAddress;
            int activePort;
            if (!mainCharon.GetExtendedActivePort(out activeCharonAddress, out activePort))
            {
                _rtuLogger.AppendLine("Can't get active port");
                return null;
            }

            Charon activeCharon = null;
            if (!activeCharonAddress.Equals(mainCharon.NetAddress))
            {
                activeCharon = mainCharon.Children.Values.First(c => c.NetAddress.Equals(activeCharonAddress));
            }
            return activeCharon;
            // end of RtuManager block of code
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
                var result = await Task.Run(() => OtdrManager.MeasureWithBase(buffer, GetActiveChildCharon()));

                IsMeasurementInProgress = false;
                if (!result)
                {
                    Message = "Measurement  error, see log";
                    return;
                }

                var lastSorDataBuffer = OtdrManager.GetLastSorDataBuffer();
                if (lastSorDataBuffer == null)
                {
                    Message = "Can't get result, see log";
                    return;
                }
                var sorData = OtdrManager.ApplyFilter(OtdrManager.ApplyAutoAnalysis(lastSorDataBuffer), OtdrManager.IsFilterOnInBase(buffer));
                sorData.Save(MeasFileName);
                Message = "Measurement is finished.";
            }
        }

        // button
        public void CompareMeasurementWithBase()
        {
            var bufferBase = File.ReadAllBytes(BaseFileName);
            var bufferMeas = File.ReadAllBytes(MeasFileName);

            var moniResult = OtdrManager.CompareMeasureWithBase(bufferBase, bufferMeas, true);
            var sorData = SorData.FromBytes(bufferMeas);
            sorData.Save(MeasFileName);

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

                    await Task.Run(() => OtdrManager.MeasureWithBase(baseBytes, GetActiveChildCharon()));

                    IsMeasurementInProgress = false;
                    Message = $"{c}th measurement is finished.";

                    var measBytes = OtdrManager.ApplyAutoAnalysis(OtdrManager.GetLastSorDataBuffer()); // is ApplyAutoAnalysis necessary ?
                    var moniResult = OtdrManager.CompareMeasureWithBase(baseBytes, measBytes, true);
                    var sorData = SorData.FromBytes(measBytes);
                    sorData.Save(MeasFileName);

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