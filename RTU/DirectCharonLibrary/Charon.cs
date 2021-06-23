﻿using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DirectCharonLibrary
{
    public partial class Charon
    {
        private readonly IniFile _iniFile35;
        private readonly IMyLog _rtuLogFile;
        private readonly int _connectionTimeout;
        private readonly int _readTimeout;
        private readonly int _writeTimeout;
        private readonly int _pauseBetweenCommands;
        private readonly int _charonIniSize;
        public NetAddress NetAddress { get; set; }
        public string Serial { get; set; }
        public int OwnPortCount { get; set; }
        public int FullPortCount { get; set; }
        public bool IsBopSupported { get; set; } // for main charon
        public bool IsOk { get; set; }

        public Dictionary<int, Charon> Children { get; set; }

        public string LastErrorMessage { get; set; }
        public string LastAnswer { get; set; }
        public bool IsLastCommandSuccessful { get; set; }

        public Charon(NetAddress netAddress, IniFile iniFile35, IMyLog rtuLogFile)
        {
            _iniFile35 = iniFile35;
            _rtuLogFile = rtuLogFile;
            _connectionTimeout = _iniFile35.Read(IniSection.Charon, IniKey.ConnectionTimeout, 5);
            _readTimeout = _iniFile35.Read(IniSection.Charon, IniKey.ReadTimeout, 2);
            _writeTimeout = _iniFile35.Read(IniSection.Charon, IniKey.WriteTimeout, 2);
            _pauseBetweenCommands = _iniFile35.Read(IniSection.Charon, IniKey.PauseBetweenCommandsMs, 200);
            _charonIniSize = _iniFile35.Read(IniSection.Charon, IniKey.CharonIniSize, 480);
            NetAddress = netAddress;
        }

        /// <summary>
        /// Initialized OTAU recursively
        /// </summary>
        /// <returns>null if initialization is successful, damaged OTAU address otherwise</returns>
        public NetAddress InitializeOtauRecursively()
        {
            _rtuLogFile.AppendLine($"Initializing OTAU on {NetAddress.ToStringA()}");
            Children = new Dictionary<int, Charon>();

            Serial = GetSerial();
            if (!IsLastCommandSuccessful)
            {
                LedDisplay.Show(_iniFile35, _rtuLogFile, LedDisplayCode.ErrorConnectOtau);
                LastErrorMessage = $"Get Serial for {NetAddress.ToStringA()} error {LastErrorMessage}";
                _rtuLogFile.AppendLine(LastErrorMessage, 2);
                return NetAddress;
            }
            Serial = Serial.Substring(0, Serial.Length - 2); // "\r\n"
            _rtuLogFile.AppendLine($"Serial {Serial}", 2);

            OwnPortCount = GetOwnPortCount();
            FullPortCount = OwnPortCount;
            if (!IsLastCommandSuccessful)
            {
                LastErrorMessage = $"Get own port count error {LastErrorMessage}";
                _rtuLogFile.AppendLine(LastErrorMessage, 2);
                return NetAddress;
            }
            _rtuLogFile.AppendLine($"Own port count  {OwnPortCount}", 2);
            IsOk = true;

            IsBopSupported = false;
            var expendedPorts = GetExtentedPorts();
            if (!IsLastCommandSuccessful)
            {
                LastErrorMessage = $"Get extended ports error {LastErrorMessage}";
                _rtuLogFile.AppendLine(LastErrorMessage, 2);
                return NetAddress;
            }
            if (LastAnswer.Substring(0, 15) == "ERROR_COMMAND\r\n")
            {
                _rtuLogFile.AppendLine("Charon too old, knows nothing about extensions", 2);
            }
            IsBopSupported = true;

            if (expendedPorts != null)
                foreach (var expendedPort in expendedPorts)
                {
                    var childCharon = new Charon(expendedPort.Value, _iniFile35, _rtuLogFile);
                    Children.Add(expendedPort.Key, childCharon); // even if it broken it should be in list

                    var childSerial = childCharon.GetSerial();
                    if (!IsLastCommandSuccessful || childSerial == "")
                    {
                        LedDisplay.Show(_iniFile35, _rtuLogFile, LedDisplayCode.ErrorConnectOtau);
                        LastErrorMessage = $"Get Serial for {expendedPort.Value.ToStringA()} error {LastErrorMessage}";
                        _rtuLogFile.AppendLine(LastErrorMessage, 2);
                    }
                    else
                    {
                        childCharon.Serial = childSerial.Substring(0, childSerial.Length - 2); // "\r\n"
                        if (childCharon.InitializeOtauRecursively() != null)
                        {
                            LedDisplay.Show(_iniFile35, _rtuLogFile, LedDisplayCode.ErrorConnectBop);
                            IsLastCommandSuccessful = true; // child initialization shouldn't break full process
                            LastErrorMessage = $"Child charon {expendedPort.Value.ToStringA()} initialization failed";
                            _rtuLogFile.AppendLine(LastErrorMessage, 2);
                            continue;
                        }
                        FullPortCount += childCharon.FullPortCount;
                    }
                }

            _rtuLogFile.AppendLine($"Full port count  {FullPortCount}");

            _rtuLogFile.AppendLine($"OTAU initialized successfully.  {Serial}  {OwnPortCount}/{FullPortCount}");
            return null;
        }

        public Dictionary<int, OtauDto> GetChildrenDto()
        {
            return Children.ToDictionary(
                pair => pair.Key,
                pair => new OtauDto()
                { Serial = pair.Value.Serial, OwnPortCount = pair.Value.OwnPortCount, NetAddress = pair.Value.NetAddress, IsOk = pair.Value.IsOk });
        }
    }

}