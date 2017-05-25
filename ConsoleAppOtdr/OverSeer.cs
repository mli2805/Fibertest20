﻿using System.IO;
using Iit.Fibertest.DirectCharonLibrary;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.Utils35;
using Iit.Fibertest.Utils35.IniFile;

namespace ConsoleAppOtdr
{
    public class OverSeer
    {
        private readonly Logger35 _logger35;
        private readonly IniFile _iniFile35;
        private OtdrManager _otdrManager;
        private Charon _mainCharon;


        private const string DefaultIp = "192.168.88.101";

        public OverSeer(Logger35 logger35, IniFile iniFile35)
        {
            _logger35 = logger35;
            _iniFile35 = iniFile35;
        }

        public bool InitializeOtdr()
        {
            _otdrManager = new OtdrManager(@"OtdrMeasEngine\", _iniFile35, _logger35);
            if (_otdrManager.LoadDll() != "")
                return false;

            var otdrAddress = _iniFile35.Read(IniSection.General, IniKey.OtdrIp, DefaultIp);
            if (_otdrManager.InitializeLibrary())
                _otdrManager.ConnectOtdr(otdrAddress);
            return _otdrManager.IsOtdrConnected;
        }

        public bool InitializeOtau()
        {
            var otauIpAddress = _iniFile35.Read(IniSection.General, IniKey.OtauIp, DefaultIp);
            _mainCharon = new Charon(new NetAddress(otauIpAddress, 23), _iniFile35, _logger35);
            return _mainCharon.Initialize();
        }

        public MoniResult MoniPort(int port, BaseRefType baseRefType)
        {
            var baseBytes = GetBase(port, baseRefType);
            _otdrManager.MeasureWithBase(baseBytes, _mainCharon.GetActiveChildCharon());
            return _otdrManager.CompareMeasureWithBase(baseBytes,
                _otdrManager.ApplyAutoAnalysis(_otdrManager.GetLastSorDataBuffer()), true); // is ApplyAutoAnalysis necessary ?
        }

        private byte[] GetBase(int port, BaseRefType baseRefType)
        {
            var basefile = $@"..\PortData\{port}\{baseRefType.ToFileName()}";
            if (File.Exists(basefile))
                return File.ReadAllBytes(basefile);
            _logger35.AppendLine($"Can't find {baseRefType.ToFileName()} for port {port}");
            return null;
        }

    }
}