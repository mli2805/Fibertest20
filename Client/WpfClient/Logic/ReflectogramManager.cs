﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Caliburn.Micro;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;
using Microsoft.Win32;
using Optixsoft.SorExaminer.OtdrDataFormat;

namespace Iit.Fibertest.Client
{
    public class ReflectogramManager
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly IWcfServiceCommonC2D _c2DWcfCommonManager;
        private readonly IWindowManager _windowManager;

        private string _tempSorFile;

        public ReflectogramManager(IniFile iniFile, IMyLog logFile, 
             IWcfServiceCommonC2D c2DWcfCommonManager, IWindowManager windowManager)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _c2DWcfCommonManager = c2DWcfCommonManager;
            _windowManager = windowManager;
        }

        public void SetTempFileName(string traceTitle, int sorFileId, DateTime timestamp)
        {
            _tempSorFile = $@"{traceTitle} - ID{sorFileId} - {timestamp:dd-MM-yyyy-HH-mm-ss}.sor";
        }

        public void SetTempFileName(string traceTitle, string baseType, DateTime timestamp)
        {
            _tempSorFile = $@"{traceTitle} - {baseType} - {timestamp:dd-MM-yyyy-HH-mm-ss}.sor";
        }

        private string SaveInTempFolderAndOpenInReflect(byte[] sorBytes)
        {
            var tempFolder = FileOperations.GetParentFolder(AppDomain.CurrentDomain.BaseDirectory) + @"\Temp\";
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
            byte[] sorbytes = SorData.GetRidOfBase(sorbytesWithBase);
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
            SaveAs(shouldBaseRefBeExcluded ? SorData.GetRidOfBase(sorbytes) : sorbytes);
        }

        public async void SaveBaseReflectogramAs(int sorFileId)
        {
            byte[] sorbytes = await GetSorBytes(sorFileId);
            SaveAs(sorbytes);
        }

        public async void ShowRftsEvents(int sorFileId, string traceTitle)
        {
            var sorbytes = await GetSorBytes(sorFileId);

            OtdrDataKnownBlocks sorData;
            var result = SorData.TryGetFromBytes(sorbytes, out sorData);
            if (result != "")
            {
                _logFile.AppendLine(result);
                return;
            }

            var vm = new RftsEventsViewModel(_windowManager);
            vm.Initialize(sorData, traceTitle);
            _windowManager.ShowWindowWithAssignedOwner(vm);
        }

        //------------------------------------------------------------------------------------------------
        public async Task<byte[]> GetSorBytes(int sorFileId)
        {
            var sorbytes = await _c2DWcfCommonManager.GetSorBytes(sorFileId);
            if (sorbytes == null)
            {
                _logFile.AppendLine($@"Cannot get reflectogram for measurement {sorFileId}");
                return new byte[0];
            }
            return sorbytes;
        }

        private void OpenSorInReflect(string sorFilename, string options = "")
        {
            var rootPath = FileOperations.GetParentFolder(AppDomain.CurrentDomain.BaseDirectory, 2);
            Process process = new Process();
            process.StartInfo.FileName = rootPath + @"\RftsReflect\Reflect.exe";
            process.StartInfo.Arguments = $@"{options} " + '"' + sorFilename + '"';
            process.Start();
        }

        private void SaveAs(byte[] sorbytes)
        {
            // if InitialDirectory for OpenFileDialog does not exist:
            //   when drive in InitialDirectory exists - it's ok - will be used previous path from Windows
            //   but if even drive does not exist will be thrown exception
            var pathToSor = FileOperations.GetParentFolder(AppDomain.CurrentDomain.BaseDirectory) + @"\tmp";

            var initialDirectory = _iniFile.Read(IniSection.Miscellaneous, IniKey.PathToSor, pathToSor);
            if (!Directory.Exists(initialDirectory))
            {
                initialDirectory = pathToSor;
                _iniFile.Write(IniSection.Miscellaneous, IniKey.PathToSor, initialDirectory);
            }
            
            var sfd = new SaveFileDialog
            {
                Filter = @"Sor file (*.sor)|*.sor",
                InitialDirectory = initialDirectory,
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
