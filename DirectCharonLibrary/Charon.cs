using System.Collections.Generic;
using Iit.Fibertest.Utils35;

namespace Iit.Fibertest.DirectCharonLibrary
{
    public partial class Charon
    {
        private readonly IniFile _iniFile35;
        private readonly Logger35 _rtuLogger35;
        private readonly CharonLogLevel _charonLogLevel;
        private readonly int _connectionTimeout;
        private readonly int _readTimeout;
        private readonly int _writeTimeout;
        public NetAddress NetAddress { get; set; }
        public string Serial { get; set; }
        public int OwnPortCount { get; set; }
        public int FullPortCount { get; set; }

        public Charon Parent { get; set; }
        public Dictionary<int, Charon> Children { get; set; }

        public string LastErrorMessage { get; set; }
        public string LastAnswer { get; set; }
        public bool IsLastCommandSuccessful { get; set; }

        public Charon(NetAddress netAddress, IniFile iniFile35, Logger35 rtuLogger35)
        {
            _iniFile35 = iniFile35;
            _rtuLogger35 = rtuLogger35;
            _charonLogLevel = (CharonLogLevel)_iniFile35.Read(IniSection.Charon, IniKey.LogLevel, 1);
            _connectionTimeout = _iniFile35.Read(IniSection.Charon, IniKey.ConnectionTimeout, 5);
            _readTimeout = _iniFile35.Read(IniSection.Charon, IniKey.ReadTimeout, 2);
            _writeTimeout = _iniFile35.Read(IniSection.Charon, IniKey.WriteTimeout, 2);
            NetAddress = netAddress;
        }

        public bool InitializeRtu()
        {
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>null if initialization is successfull, damaged otau address otherwise</returns>
        public NetAddress InitializeOtau()
        {
            _rtuLogger35.AppendLine($"Initializing OTAU on {NetAddress.ToStringA()}");
            Children = new Dictionary<int, Charon>();

            Serial = GetSerial();
            if (!IsLastCommandSuccessful)
            {
                LedDisplay.Show(_iniFile35, _rtuLogger35, LedDisplayCode.ErrorConnectOtau);
                LastErrorMessage = $"Get Serial error {LastErrorMessage}";
                if (_charonLogLevel >= CharonLogLevel.PublicCommands)
                    _rtuLogger35.AppendLine(LastErrorMessage, 2);
                return NetAddress;
            }
            Serial = Serial.Substring(0, Serial.Length - 2); // "\r\n"
            if (_charonLogLevel >= CharonLogLevel.PublicCommands)
                _rtuLogger35.AppendLine($"Serial {Serial}", 2);

            OwnPortCount = GetOwnPortCount();
            FullPortCount = OwnPortCount;
            if (!IsLastCommandSuccessful)
            {
                LastErrorMessage = $"Get own port count error {LastErrorMessage}";
                if (_charonLogLevel >= CharonLogLevel.PublicCommands)
                    _rtuLogger35.AppendLine(LastErrorMessage, 2);
                return NetAddress;
            }
            if (_charonLogLevel >= CharonLogLevel.PublicCommands)
                _rtuLogger35.AppendLine($"Own port count  {OwnPortCount}", 2);

            var expendedPorts = GetExtentedPorts();
            if (!IsLastCommandSuccessful)
            {
                LastErrorMessage = $"Get extended ports error {LastErrorMessage}";
                if (_charonLogLevel >= CharonLogLevel.PublicCommands)
                    _rtuLogger35.AppendLine(LastErrorMessage, 2);
                return NetAddress;
            }
            if (expendedPorts != null)
                foreach (var expendedPort in expendedPorts)
                {
                    var childCharon = new Charon(expendedPort.Value, _iniFile35, _rtuLogger35);
                    childCharon.Parent = this;
                    Children.Add(expendedPort.Key, childCharon);
                    if (childCharon.InitializeOtau() != null)
                    {
                        LedDisplay.Show(_iniFile35, _rtuLogger35, LedDisplayCode.ErrorConnectBop);
                        IsLastCommandSuccessful = true; // child initialization should'n break full process
                        LastErrorMessage = $"Child charon {expendedPort.Value.ToStringA()} initialization failed";
                        if (_charonLogLevel >= CharonLogLevel.PublicCommands)
                            _rtuLogger35.AppendLine(LastErrorMessage, 2);
                        return childCharon.NetAddress;
                    }
                    FullPortCount += childCharon.FullPortCount;
                }

            if (_charonLogLevel >= CharonLogLevel.PublicCommands)
                _rtuLogger35.AppendLine($"Full port count  {FullPortCount}");

            _rtuLogger35.AppendLine($"OTAU initialized successfully.  {Serial}  {OwnPortCount}/{FullPortCount}");
            return null;
        }
    }
}