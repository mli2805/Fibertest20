﻿using System.Diagnostics;
using System.IO;
using Caliburn.Micro;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;
using Iit.Fibertest.WpfCommonViews;
using Optixsoft.SorExaminer.OtdrDataFormat;

namespace Iit.Fibertest.Client
{
    public class MeasurementManager
    {
        private readonly IMyLog _logFile;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly IWindowManager _windowManager;

        public MeasurementManager(IMyLog logFile, IWcfServiceForClient c2DWcfManager, IWindowManager windowManager)
        {
            _logFile = logFile;
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;
        }

        public async void ShowRefWithBase(int sorFileId)
        {
            var sorbytes = await _c2DWcfManager.GetSorBytesOfMeasurement(sorFileId);
            if (sorbytes == null)
            {
                _logFile.AppendLine($@"Cannot get reflectogram for measurement {sorFileId}");
                return;
            }
            var assemblyFilename = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var assemblyFolder = Path.GetDirectoryName(assemblyFilename) ?? @"c:\";
            var tempFolder = Path.Combine(assemblyFolder, @"..\Temp\");
            if (!Directory.Exists(tempFolder))
                Directory.CreateDirectory(tempFolder);
            var sorFilename = Path.Combine(tempFolder, @"meas.sor");
            File.WriteAllBytes(sorFilename, sorbytes);

            Process process = new Process();
            process.StartInfo.FileName = Path.Combine(assemblyFolder, @"..\..\RFTSReflect\reflect.exe");
            process.StartInfo.Arguments = sorFilename;
            process.Start();
        }
        public async void ShowReflectogram(int sorFileId)
        {
        }

        public async void SaveRefWithBase(int sorFileId)
        {
            
        }

        public async void SaveReflectogram(int sorFileId)
        {
            
        }

        public async void ShowRftsEvents(int sorFileId)
        {
            var sorbytes = await _c2DWcfManager.GetSorBytesOfMeasurement(sorFileId);
            if (sorbytes == null)
            {
                _logFile.AppendLine($@"Cannot get reflectogram for measurement {sorFileId}");
                return;
            }

            OtdrDataKnownBlocks sorData;
            var result = SorData.TryGetFromBytes(sorbytes, out sorData);
            if (result != "")
            {
                _logFile.AppendLine(result);
                return;
            }

            var vm = new RftsEventsViewModel(sorData);
            _windowManager.ShowWindow(vm);
        }
    }
}
