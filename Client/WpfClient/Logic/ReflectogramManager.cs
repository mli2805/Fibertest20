using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;
using Iit.Fibertest.WpfCommonViews;
using Microsoft.Win32;
using Optixsoft.SorExaminer.OtdrDataFormat;

namespace Iit.Fibertest.Client
{
    public class ReflectogramManager
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly IWindowManager _windowManager;

        private string _tempSorFile;

        public ReflectogramManager(IniFile iniFile, IMyLog logFile, IWcfServiceForClient c2DWcfManager, IWindowManager windowManager)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;
        }

        public void SetTempFileName(string traceTitle, int sorFileId, DateTime timestamp)
        {
            _tempSorFile = $@"{traceTitle} - ID{sorFileId} - {timestamp:dd-MM-yyyy-HH-mm-ss}";
        }

        public void SetTempFileName(string traceTitle, string baseType, DateTime timestamp)
        {
            _tempSorFile = $@"{traceTitle} - {baseType} - {timestamp:dd-MM-yyyy-HH-mm-ss}";
        }

        private string SaveInTempFolderAndOpenInReflect(byte[] sorBytes)
        {
            var tempFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\Temp\");
            if (!Directory.Exists(tempFolder))
                Directory.CreateDirectory(tempFolder);
            var fullFilename = Path.Combine(tempFolder, _tempSorFile);
            File.WriteAllBytes(fullFilename, sorBytes);
            return fullFilename;
        }

        public async void ShowRefWithBase(int sorFileId)
        {
            byte[] sorbytes = await GetSorBytes(sorFileId);
            var fullFilename = SaveInTempFolderAndOpenInReflect(sorbytes);
            OpenSorInReflect(fullFilename);
        }

        public async void ShowOnlyCurrentMeasurement(int sorFileId)
        {
            byte[] sorbytesWithBase = await GetSorBytes(sorFileId);
            byte[] sorbytes = GetRidOfBase(sorbytesWithBase);
            var fullFilename = SaveInTempFolderAndOpenInReflect(sorbytes);
            OpenSorInReflect(fullFilename);
        }

        public async void ShowBaseReflectogram(int sorFileId)
        {
            byte[] sorbytes = await GetSorBytes(sorFileId);
            var fullFilename = SaveInTempFolderAndOpenInReflect(sorbytes);
            OpenSorInReflect(fullFilename);
        }
        public async void ShowBaseReflectogramWithSelectedLandmark(int sorFileId, int lmNumber)
        {
            byte[] sorbytes = await GetSorBytes(sorFileId);
            var fullFilename = SaveInTempFolderAndOpenInReflect(sorbytes);
            OpenSorInReflect(fullFilename, $@"-lm {lmNumber}");
        }

        public async void SaveReflectogramAs(int sorFileId, bool shouldBaseRefBeExcluded)
        {
            byte[] sorbytes = await GetSorBytes(sorFileId);
            SaveAs(shouldBaseRefBeExcluded ? GetRidOfBase(sorbytes) : sorbytes);
        }

        public async void SaveBaseReflectogramAs(int sorFileId)
        {
            byte[] sorbytes = await GetSorBytes(sorFileId);
            SaveAs(sorbytes);
        }

        public async void ShowRftsEvents(int sorFileId)
        {
            var sorbytes = await GetSorBytes(sorFileId);

            OtdrDataKnownBlocks sorData;
            var result = SorData.TryGetFromBytes(sorbytes, out sorData);
            if (result != "")
            {
                _logFile.AppendLine(result);
                return;
            }

            var vm = new RftsEventsViewModel(sorData);
            _windowManager.ShowWindowWithAssignedOwner(vm);
        }

        //------------------------------------------------------------------------------------------------
        private async Task<byte[]> GetSorBytes(int sorFileId)
        {
            var sorbytes = await _c2DWcfManager.GetSorBytes(sorFileId);
            if (sorbytes == null)
            {
                _logFile.AppendLine($@"Cannot get reflectogram for measurement {sorFileId}");
                return new byte[0];
            }
            return sorbytes;
        }

        private byte[] GetRidOfBase(byte[] sorbytes)
        {
            var result = SorData.TryGetFromBytes(sorbytes, out var otdrDataKnownBlocks);
            if (result != "")
            {
                _logFile.AppendLine(result);
                return new byte[0];
            }

            var n = otdrDataKnownBlocks.EmbeddedData.EmbeddedDataBlocks.Where(block => block.Description != @"SOR").ToArray();
            otdrDataKnownBlocks.EmbeddedData.EmbeddedDataBlocks = n;
            return otdrDataKnownBlocks.ToBytes();
        }

        private void OpenSorInReflect(string sorFilename, string options = "")
        {
            Process process = new Process();
            process.StartInfo.FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\RFTSReflect\reflect.exe");
            process.StartInfo.Arguments = $"{options} " + '"' + sorFilename + '"';
            process.Start();
        }

        private void SaveAs(byte[] sorbytes)
        {
            var sfd = new SaveFileDialog
            {
                Filter = @"Sor file (*.sor)|*.sor",
                InitialDirectory = _iniFile.Read(IniSection.Miscellaneous, IniKey.PathToSor, @"c:\temp\"),
                FileName = _tempSorFile,
            };
            if (sfd.ShowDialog() == true)
            {
                var path = Path.GetDirectoryName(sfd.FileName);
                _iniFile.Write(IniSection.Miscellaneous, IniKey.PathToSor, path);
                File.WriteAllBytes(sfd.FileName, sorbytes);
            }
        }
    }
}
