using System;
using System.IO;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WpfCommonViews;
using Microsoft.Win32;
using Optixsoft.SorExaminer.OtdrDataFormat;

namespace DirectRtuClient
{
    public class ParseViewModel : Screen
    {
        private readonly IMyLog _logFile;

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

        private string _baseFileName;
        public string BaseFileName
        {
            get => _baseFileName;
            set
            {
                if (value == _baseFileName) return;
                _baseFileName = value;
                NotifyOfPropertyChange();
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

        private readonly OtdrManager _otdrManager;

        public ParseViewModel(IniFile iniFile35, IMyLog logFile)
        {
            _logFile = logFile;

            _otdrManager = new OtdrManager(@"OtdrMeasEngine\", iniFile35, _logFile);
            var initializationResult = _otdrManager.LoadDll();
            if (initializationResult != "")
                InitializationMessage = initializationResult;
            else InitializationMessage = @"Dlls loaded successfully!";
            IsLibraryInitialized = _otdrManager.InitializeLibrary();
        }

        public void Compare()
        {
            var baseBytes = LoadSorFile(BaseFileName);
            // var baseSorData = SorData.FromBytes(baseBytes);

            var measBytes = LoadSorFile(ResultFileName);

            var moniResult = _otdrManager.CompareMeasureWithBase(baseBytes, measBytes, true);
            var measWithBase = SorData.FromBytes(moniResult.SorBytes);
            // measWithBase.Save(@"c:\temp\sor\after-compare.sor");

            IWindowManager windowManager = new WindowManager();
            var vm = new RftsEventsViewModel(windowManager);
            vm.Initialize(measWithBase);
            windowManager.ShowDialog(vm);
        }

        public void Parse()
        {
            var bytes = LoadSorFile(ResultFileName);
            var sorData = SorData.FromBytes(bytes);

            LogKeyEvents(sorData);

            LogLandmarks(sorData);
        }

        private void LogLandmarks(OtdrDataKnownBlocks sorData)
        {
            _logFile.AppendLine("");
            _logFile.AppendLine(@"Landmarks");
            _logFile.AppendLine("");
            foreach (var landmark in sorData.LinkParameters.LandmarkBlocks)
                _logFile.AppendLine(landmark.Number - 1 + @":  " + landmark.Comment);
        }

        private void LogKeyEvents(OtdrDataKnownBlocks sorData)
        {
            _logFile.AppendLine("");
            _logFile.AppendLine(@"Key events");
            _logFile.AppendLine("");
            foreach (var keyEvent in sorData.KeyEvents.KeyEvents)
                _logFile.AppendLine(keyEvent.EventNumber - 1 + @":  " + keyEvent.Comment);
        }

        public void ChooseBaseFilename()
        {
            var fd = new OpenFileDialog();
            fd.Filter = @"Sor files (*.sor)|*.sor";
            fd.InitialDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\out\"));
            if (fd.ShowDialog() == true)
                BaseFileName = fd.FileName;
        }

        public void ChooseResultFilename()
        {
            var fd = new OpenFileDialog();
            fd.Filter = @"Sor files (*.sor)|*.sor";
            fd.InitialDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\out\"));
            if (fd.ShowDialog() == true)
                ResultFileName = fd.FileName;
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

    }
}
