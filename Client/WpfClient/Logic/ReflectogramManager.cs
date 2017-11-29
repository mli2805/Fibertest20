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

        private readonly string _assemblyFolder;
        private string _tempSorFile;

        public ReflectogramManager(IniFile iniFile, IMyLog logFile, IWcfServiceForClient c2DWcfManager, IWindowManager windowManager)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;

            _assemblyFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? @"c:\";
            SetTempSorFileName();
        }

        public async void ShowRefWithBase(int sorFileId)
        {
            byte[] sorbytes = await GetSorBytes(sorFileId);
            File.WriteAllBytes(_tempSorFile, sorbytes);
            OpenSorInReflect(_tempSorFile);
        }

        public async void ShowOnlyCurrentMeasurement(int sorFileId)
        {
            byte[] sorbytes = await GetSorBytes(sorFileId);
            byte[] sorbytesWithoutBase = GetRidOfBase(sorbytes);
            File.WriteAllBytes(_tempSorFile, sorbytesWithoutBase);
            OpenSorInReflect(_tempSorFile);
        }


        public async void SaveReflectogramAs(int sorFileId, string defaultFilename, bool shouldBaseRefBeExcluded)
        {
            byte[] sorbytes = await GetSorBytes(sorFileId);
            SaveAs(shouldBaseRefBeExcluded ? GetRidOfBase(sorbytes) : sorbytes, defaultFilename);
        }

        public async void ShowBaseReflectogram(Guid baseRefId)
        {
            byte[] sorbytes = await GetBaseSorBytes(baseRefId);
            File.WriteAllBytes(_tempSorFile, sorbytes);
            OpenSorInReflect(_tempSorFile);
        }

        public async void SaveBaseReflectogramAs(Guid baseRefId, string partFilename)
        {
            byte[] sorbytes = await GetBaseSorBytes(baseRefId);

            OtdrDataKnownBlocks sorData;
            var result = SorData.TryGetFromBytes(sorbytes, out sorData);
            if (result != "")
            {
                _logFile.AppendLine(result);
                return;
            }

            
            var timestamp = $@"{sorData.IitParameters.LocalTimeStamp:dd-MM-yyyy HH-mm-ss}";
            
            SaveAs(sorbytes, partFilename + timestamp);
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



        //------------------------------------------------------------------------------------------------

        private async Task<byte[]> GetSorBytes(int sorFileId)
        {
            var sorbytes = await _c2DWcfManager.GetSorBytesOfMeasurement(sorFileId);
            if (sorbytes == null)
            {
                _logFile.AppendLine($@"Cannot get reflectogram for measurement {sorFileId}");
                return new byte[0];
            }
            return sorbytes;
        }

        private async Task<byte[]> GetBaseSorBytes(Guid baseRefId)
        {
            var sorbytes = await _c2DWcfManager.GetSorBytesOfBase(baseRefId);
            if (sorbytes == null)
            {
                _logFile.AppendLine($@"Cannot get base reflectogram {baseRefId}");
                return new byte[0];
            }
            return sorbytes;
        }

        private byte[] GetRidOfBase(byte[] sorbytes)
        {
            OtdrDataKnownBlocks otdrDataKnownBlocks;
            var result = SorData.TryGetFromBytes(sorbytes, out otdrDataKnownBlocks);
            if (result != "")
            {
                _logFile.AppendLine(result);
                return new byte[0];
            }

            var n = otdrDataKnownBlocks.EmbeddedData.EmbeddedDataBlocks.Where(block => block.Description != @"SOR").ToArray();
            otdrDataKnownBlocks.EmbeddedData.EmbeddedDataBlocks = n;
            return SorData.ToBytes(otdrDataKnownBlocks);
        }

        private void SetTempSorFileName()
        {
            var tempFolder = Path.Combine(_assemblyFolder, @"..\Temp\");
            if (!Directory.Exists(tempFolder))
                Directory.CreateDirectory(tempFolder);
            _tempSorFile = Path.Combine(tempFolder, @"meas.sor");
        }

        private void OpenSorInReflect(string sorFilename)
        {
            Process process = new Process();
            process.StartInfo.FileName = Path.Combine(_assemblyFolder, @"..\..\RFTSReflect\reflect.exe");
            process.StartInfo.Arguments = '"' + sorFilename + '"';
            process.Start();
        }

        private void SaveAs(byte[] sorbytes, string defaultFileName)
        {
            var sfd = new SaveFileDialog
            {
                Filter = @"Sor file (*.sor)|*.sor",
                InitialDirectory = _iniFile.Read(IniSection.Miscellaneous, IniKey.PathToSor, @"c:\temp\"),
                FileName = defaultFileName,
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
