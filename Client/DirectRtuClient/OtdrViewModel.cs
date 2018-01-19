using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.DirectCharonLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WpfCommonViews;
using Microsoft.Win32;

namespace DirectRtuClient
{
    public class OtdrViewModel : Screen
    {
        private readonly IniFile _iniFile35;
        private readonly IMyLog _rtuLogger;
    //    private readonly string _appDir;
        public string IpAddress { get; set; }

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

        public OtdrViewModel(IniFile iniFile35, IMyLog rtuLogger, string ipAddress)
        {
            _iniFile35 = iniFile35;
            _rtuLogger = rtuLogger;
            IpAddress = ipAddress;

            BaseFileName = @"..\out\base3ev.sor";
            MeasFileName = @"..\out\123.sor";
            ResultFileName = @"..\out\measwithbase.sor";


            OtdrManager = new OtdrManager(@"OtdrMeasEngine\", _iniFile35, _rtuLogger);
            var initializationResult = OtdrManager.LoadDll();
            if (initializationResult != "")
                InitializationMessage = initializationResult;
            IsLibraryInitialized = OtdrManager.InitializeLibrary();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_OTDR;
        }

        public async Task ConnectOtdr()
        {
            InitializationMessage = Resources.SID_Wait__please___;

            await ConnectionProcess();

            InitializationMessage = IsOtdrConnected ? Resources.SID_OTDR_connected_successfully_ : Resources.SID_OTDR_connection_failed_;
        }

        private async Task ConnectionProcess() // button
        {
            using (new WaitCursor())
            {
                await Task.Run(() => OtdrManager.ConnectOtdr(IpAddress));
                IsOtdrConnected = OtdrManager.IsOtdrConnected;
            }
        }

        public void LaunchOtdrParamView()
        {
            var vm = new OtdrParametersDirectSetterViewModel(OtdrManager.InterOpWrapper);
            IWindowManager windowManager = new WindowManager();
            windowManager.ShowDialog(vm);
        }

        private CancellationTokenSource _cancellationTokenSource;
        public async Task StartMeasurement()
        {
            using (new WaitCursor())
            {
                IsMeasurementInProgress = true;
                Message = Resources.SID_Wait__please___;

                _cancellationTokenSource = new CancellationTokenSource();
                await Task.Run(() => OtdrManager.DoManualMeasurement(_cancellationTokenSource, ShouldForceLmax, GetActiveChildCharon()));

                IsMeasurementInProgress = false;
                Message = Resources.SID_Measurement_is_finished_;

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
            mainCharon.InitializeOtau();
            NetAddress activeCharonAddress;
            if (!mainCharon.GetExtendedActivePort(out activeCharonAddress, out _))
            {
                _rtuLogger.AppendLine(Resources.SID_Can_t_get_active_port);
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
            fd.Filter = @"Sor files (*.sor)|*.sor";
            //            fd.InitialDirectory = @"c:\temp\";
            fd.InitialDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\out\"));
            if (fd.ShowDialog() == true)
                BaseFileName = fd.FileName;
        }

        public void ChooseResultFilename()
        {
            var fd = new OpenFileDialog();
            fd.Filter = @"Sor files (*.sor)|*.sor";
            //            fd.InitialDirectory = @"c:\temp\";
            fd.InitialDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\out\"));
            if (fd.ShowDialog() == true)
                ResultFileName = fd.FileName;
        }

        public void ChooseMeasFilename()
        {
            var fd = new SaveFileDialog();
            fd.Filter = @"Sor files (*.sor)|*.sor";
            //            fd.InitialDirectory = @"c:\temp\";
            fd.InitialDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\out\"));
            if (fd.ShowDialog() == true)
                MeasFileName = fd.FileName;
        }

        public async Task StartMeasurementWithBase()
        {
            using (new WaitCursor())
            {
                IsMeasurementInProgress = true;
                Message = Resources.SID_Wait__please___;

                byte[] baseBytes = File.ReadAllBytes(BaseFileName);
                _cancellationTokenSource = new CancellationTokenSource();
                var result = await Task.Run(() => OtdrManager.MeasureWithBase(_cancellationTokenSource, baseBytes, GetActiveChildCharon()));

                IsMeasurementInProgress = false;
                if (result == ReturnCode.MeasurementInterrupted)
                {
                    Message = @"Measurement interrupted";
                    return;
                }
                if (result != ReturnCode.MeasurementEndedNormally)
                {
                    Message = Resources.SID_Measurement_error__see_log;
                    return;
                }

                var lastSorDataBuffer = OtdrManager.GetLastSorDataBuffer();
                if (lastSorDataBuffer == null)
                {
                    Message = Resources.SID_Can_t_get_result__see_log;
                    return;
                }
                var sorData = OtdrManager.ApplyFilter(OtdrManager.ApplyAutoAnalysis(lastSorDataBuffer), OtdrManager.IsFilterOnInBase(baseBytes));
                sorData.Save(MeasFileName);
                Message = Resources.SID_Measurement_is_finished_;
            }
        }

        public void Analyze()
        {
            var baseBytes = LoadSorFile(BaseFileName);
            if (baseBytes == null) return;
            var measBytes = LoadSorFile(MeasFileName);
            if (measBytes == null) return;
            var sorData = OtdrManager.ApplyFilter(OtdrManager.ApplyAutoAnalysis(measBytes), OtdrManager.IsFilterOnInBase(baseBytes));
            sorData.Save(MeasFileName);
            Message = Resources.SID_Analysis_is_finished;
        }

        private byte[] LoadSorFile(string filename)
        {
            try
            {
                return File.ReadAllBytes(filename);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return null;
            }
        }

        // button
        public void CompareMeasurementWithBase()
        {
            var baseBytes = LoadSorFile(BaseFileName);
            if (baseBytes == null) return;
            var measBytes = LoadSorFile(MeasFileName);
            if (measBytes == null) return;

            var moniResult = OtdrManager.CompareMeasureWithBase(baseBytes, measBytes, true);
            var sorData = SorData.FromBytes(moniResult.SorBytes);
            sorData.Save(MeasFileName);

            _rtuLogger.AppendLine(string.Format(Resources.SID_Comparison_end__, moniResult.IsFiberBreak, moniResult.IsNoFiber));
        }


        public async Task StartCycle()
        {
            int c = 0;
            byte[] baseBytes = File.ReadAllBytes(BaseFileName);

            while (true)
            {
                using (new WaitCursor())
                {
                    IsMeasurementInProgress = true;
                    Message = string.Format(Resources.SID_Monitoring_cycle__0___Wait__please___, c);
                    _rtuLogger.AppendLine(string.Format(Resources.SID_Monitoring_cycle__0__, c));

                _cancellationTokenSource = new CancellationTokenSource();
                    var result = await Task.Run(() => OtdrManager.MeasureWithBase(_cancellationTokenSource, baseBytes, GetActiveChildCharon()));

                    IsMeasurementInProgress = false;
                    if (result != ReturnCode.MeasurementEndedNormally)
                        return;
                    //await Task.Run(() => ProcessMeasurementResult(baseBytes, c));
                    ProcessMeasurementResult(baseBytes, c);
                }
                c++;
            }
        }

        private void ProcessMeasurementResult(byte[] baseBytes, int count)
        {
            Message = string.Format(Resources.SID__0_th_measurement_is_finished_, count);

            var measBytes = OtdrManager.ApplyAutoAnalysis(OtdrManager.GetLastSorDataBuffer()); // does ApplyAutoAnalysis necessary ?
            var moniResult = OtdrManager.CompareMeasureWithBase(baseBytes, measBytes, true);
            var sorData = SorData.FromBytes(moniResult.SorBytes);
            sorData.Save(MeasFileName);

        }

        public void StopCycle()
        {
            _cancellationTokenSource?.Cancel();
            Message = Resources.SID_Stop_command_is_sent;
        }


        public void InterruptMeasurement()
        {
            _cancellationTokenSource?.Cancel();
            Message = Resources.SID_Stop_command_is_sent;
        }

        public void ShowRftsEvents()
        {
            if (!File.Exists(ResultFileName))
            {
                MessageBox.Show(@"There's no such file");
                return;
            }
            var buffer = File.ReadAllBytes(ResultFileName);
            var sorData = SorData.FromBytes(buffer);
            if (sorData.RftsEvents.MonitoringResult == (int)ComparisonReturns.NoFiber)
            {
                MessageBox.Show(Resources.SID_No_Fiber_, Resources.SID_Events);
                return;
            }
            var vm = new RftsEventsViewModel(sorData);
            IWindowManager windowManager = new WindowManager();
            windowManager.ShowDialog(vm);
        }

      
    }
}
