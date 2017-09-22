﻿using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using Dto;
using Iit.Fibertest.DirectCharonLibrary;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.UtilsLib;

namespace RtuManagement
{
    public partial class RtuManager
    {
        private CharonOperationResult InitializeRtuManager()
        {
            LedDisplay.Show(_rtuIni, _rtuLog, LedDisplayCode.Connecting);

            if (!InitializeOtdr())
            {
                // We can't be here because InitializeOtdr should endlessly continue
                // until Otdr is initialized Ok.
                // (It's senseless to work without OTDR.)
                _rtuLog.AppendLine("Otdr initialization failed.");
                return CharonOperationResult.OtdrError;
            }

            var result = InitializeOtau();

            _mainCharon.ShowOnDisplayMessageReady();

            _monitoringQueue = new MonitoringQueue(_rtuLog);
            _monitoringQueue.Load();
            GetMonitoringParams();

            LoadMoniResultsFromDisk();

            _rtuLog.AppendLine("Rtu Manager initialized successfully.");
            return result;
        }

        private bool InitializeOtdr()
        {
            _otdrManager = new OtdrManager(@"OtdrMeasEngine\", _rtuIni, _rtuLog);
            if (_otdrManager.LoadDll() != "")
                return false;

            if (!_otdrManager.InitializeLibrary())
                return false;

            var otdrAddress = _rtuIni.Read(IniSection.General, IniKey.OtdrIp, DefaultIp);
            Thread.Sleep(3000);
            var res = _otdrManager.ConnectOtdr(otdrAddress);
            if (!res)
            {
                RunMainCharonRecovery(); // one of recovery steps inevitably exits process
                res = _otdrManager.ConnectOtdr(otdrAddress);
                if (!res)
                    RunMainCharonRecovery(); // one of recovery steps inevitably exits process
            }
            _rtuIni.Write(IniSection.Recovering, IniKey.RecoveryStep, 0);
            return true;
        }

        private CharonOperationResult InitializeOtau()
        {
            var otauIpAddress = _rtuIni.Read(IniSection.General, IniKey.OtauIp, DefaultIp);
            _mainCharon = new Charon(new NetAddress(otauIpAddress, 23), _rtuIni, _rtuLog);
            var res = _mainCharon.InitializeOtau();

            if (res == null)
                return CharonOperationResult.Ok;

            if (!res.Equals(_mainCharon.NetAddress))
            {
                RunAdditionalOtauRecovery(null, res.Ip4Address);
                return CharonOperationResult.Ok;
            }
            else // main charon
            {
                RunMainCharonRecovery(); // one of recovery steps inevitably exits process
                res = _mainCharon.InitializeOtau();
                if (res != null)
                    RunMainCharonRecovery(); // one of recovery steps inevitably exits process
            }
            return CharonOperationResult.Ok;
        }
      
        private void GetMonitoringParams()
        {
            _preciseMakeTimespan =
                TimeSpan.FromSeconds(_rtuIni.Read(IniSection.Monitoring, IniKey.PreciseMakeTimespan, 3600));
            _preciseSaveTimespan =
                TimeSpan.FromSeconds(_rtuIni.Read(IniSection.Monitoring, IniKey.PreciseSaveTimespan, 3600));
            _fastSaveTimespan =
                TimeSpan.FromSeconds(_rtuIni.Read(IniSection.Monitoring, IniKey.FastSaveTimespan, 3600));
        }

        private void DisconnectOtdr()
        {
            var otdrAddress = _rtuIni.Read(IniSection.General, IniKey.OtdrIp, DefaultIp);
            _otdrManager.DisconnectOtdr(otdrAddress);
            _rtuLog.AppendLine("Rtu is in MANUAL mode.");
        }

        private void LoadMoniResultsFromDisk()
        {
            QueueOfMoniResultsOnDisk = new ConcurrentQueue<MoniResultOnDisk>();

            try
            {
                if (!Directory.Exists(GetStorePath()))
                    Directory.CreateDirectory(GetStorePath());
                DirectoryInfo info = new DirectoryInfo(GetStorePath());
                FileInfo[] files = info.GetFiles().OrderBy(p => p.CreationTime).ToArray();
                _serviceLog.AppendLine($"There're {files.Length} stored files");

                foreach (FileInfo file in files)
                {
                    PlaceMoniresultInQueue(file);
                }
                _serviceLog.AppendLine($"There're {QueueOfMoniResultsOnDisk.Count} moniresults in queue");

            }
            catch (Exception e)
            {
                _serviceLog.AppendLine(e.Message);
            }
        }

        private void PlaceMoniresultInQueue(FileInfo file)
        {
            var filename = Path.GetFileNameWithoutExtension(file.Name);
            _serviceLog.AppendLine(filename);
            Guid id;
            if (!Guid.TryParse(filename, out id))
                return;

            var moniResult = new MoniResultOnDisk(id, null, _serviceLog);
            moniResult.Load();
            QueueOfMoniResultsOnDisk.Enqueue(moniResult);
        }

        private string GetStorePath()
        {
            var app = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var appPath = Path.GetDirectoryName(app);
            var storePath = appPath + @"\..\ResultStore\";
            _serviceLog.AppendLine($"{storePath}");
            return storePath;
        }
    }
}
