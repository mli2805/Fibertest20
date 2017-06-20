using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        public int StartPortNumber { get; set; }


        public Charon Parent { get; set; }
        public Dictionary<int, Charon> Children { get; set; }

        public string LastErrorMessage { get; set; }
        public string LastAnswer { get; set; }
        public bool IsLastCommandSuccessful { get; set; }

        public Charon(NetAddress netAddress, IniFile iniFile35, Logger35 rtuLogger35)
        {
            _iniFile35 = iniFile35;
            _rtuLogger35 = rtuLogger35;
            _charonLogLevel = (CharonLogLevel)_iniFile35.Read(IniSection.General, IniKey.LogLevel, 2);
            _connectionTimeout = _iniFile35.Read(IniSection.Charon, IniKey.ConnectionTimeout, 5);
            _readTimeout = _iniFile35.Read(IniSection.Charon, IniKey.ReadTimeout, 2);
            _writeTimeout = _iniFile35.Read(IniSection.Charon, IniKey.WriteTimeout, 2);
            NetAddress = netAddress;
        }

        public bool InitializeRtu()
        {
            if (!ResetOtau())
                return false;

            var temp = ResetOtdr(false);

            if (!temp)
                return false;

            return Initialize();
        }

        public bool Initialize()
        {
            _rtuLogger35.AppendLine($"Initializing OTAU on {NetAddress.ToStringA()}");
            StartPortNumber = Parent == null ? 1 : StartPortNumber = Parent.FullPortCount + 1;
            Children = new Dictionary<int, Charon>();

            Serial = GetSerial();
            if (!IsLastCommandSuccessful)
            {
                LastErrorMessage = $"Get Serial error {LastErrorMessage}";
                if (_charonLogLevel >= CharonLogLevel.PublicCommands)
                    _rtuLogger35.AppendLine(LastErrorMessage, 2);
                return false;
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
                return false;
            }
            if (_charonLogLevel >= CharonLogLevel.PublicCommands)
                _rtuLogger35.AppendLine($"Own port count  {OwnPortCount}", 2);

            var expendedPorts = GetExtentedPorts();
            if (!IsLastCommandSuccessful)
            {
                LastErrorMessage = $"Get extended ports error {LastErrorMessage}";
                if (_charonLogLevel >= CharonLogLevel.PublicCommands)
                    _rtuLogger35.AppendLine(LastErrorMessage, 2);
                return false;
            }
            if (expendedPorts != null)
                foreach (var expendedPort in expendedPorts)
                {
                    var childCharon = new Charon(expendedPort.Value, _iniFile35, _rtuLogger35);
                    childCharon.Parent = this;
                    if (!childCharon.Initialize())
                    {
                        IsLastCommandSuccessful = true; // child initialization should'n break full process
                        LastErrorMessage = $"Child charon {expendedPort.Value.ToStringA()} initialization failed";
                        if (_charonLogLevel >= CharonLogLevel.PublicCommands)
                            _rtuLogger35.AppendLine(LastErrorMessage, 2);
                    }
                    Children.Add(expendedPort.Key, childCharon);
                    FullPortCount += childCharon.FullPortCount;
                }

            ShowOnBopDisplayMessageReady();

            if (_charonLogLevel >= CharonLogLevel.PublicCommands)
                _rtuLogger35.AppendLine($"Full port count  {FullPortCount}");
            else
                _rtuLogger35.AppendLine($"OTAU initialized successfully.  {Serial}  {OwnPortCount}/{FullPortCount}");
            return true;
        }

        public bool GetExtendedActivePort(out NetAddress charonAddress, out int port)
        {
            var activePort = GetActivePort();
            if (!Children.ContainsKey(activePort))
            {
                charonAddress = NetAddress;
                port = activePort;
                return true;
            }

            var activeCharon = Children[activePort];
            return activeCharon.GetExtendedActivePort(out charonAddress, out port);
        }

        public Charon GetActiveChildCharon()
        {
            var activePort = GetActivePort();
            if (!Children.ContainsKey(activePort))
            {
                return null;
            }
            return Children[activePort];
        }

        public bool SetExtendedActivePort(NetAddress charonAddress, int port)
        {
            _rtuLogger35.AppendLine($"Toggling to port {port} on {charonAddress.ToStringA()}...");
            if (NetAddress.Equals(charonAddress))
            {
                var activePort = SetActivePort(port);
                _rtuLogger35.AppendLine("Toggled Ok.");
                return activePort == port;
            }

            var charon = Children.Values.FirstOrDefault(c => c.NetAddress.Equals(charonAddress));
            if (charon == null)
            {
                LastErrorMessage = "There is no such optical switch";
                if (_charonLogLevel >= CharonLogLevel.PublicCommands)
                    _rtuLogger35.AppendLine(LastErrorMessage, 2);
                return false;
            }

            var masterPort = Children.First(pair => pair.Value == charon).Key;
            if (GetActivePort() != masterPort)
            {
                var newMasterPort = SetActivePort(masterPort);
                if (newMasterPort != masterPort)
                {
                    LastErrorMessage = $"Can't toggle master switch into {masterPort} port";
                    if (_charonLogLevel >= CharonLogLevel.PublicCommands)
                        _rtuLogger35.AppendLine(LastErrorMessage, 2);
                    return false;
                }
            }

            var resultingPort = charon.SetActivePort(port);
            if (resultingPort != port)
            {
                LastErrorMessage = charon.LastErrorMessage;
                IsLastCommandSuccessful = charon.IsLastCommandSuccessful;
                if (_charonLogLevel >= CharonLogLevel.PublicCommands)
                    _rtuLogger35.AppendLine(LastErrorMessage, 2);
                return false;
            }

            _rtuLogger35.AppendLine("Toggled Ok.");
            return true;
        }

        public int SetExtendedActivePort(int port)
        {
            if (port <= OwnPortCount)
                return SetActivePort(port);

            Charon child = Children.Values.FirstOrDefault(
                    c => c.StartPortNumber <= port && c.StartPortNumber + c.OwnPortCount > port);
            if (child == null)
            {
                LastErrorMessage = "Out of range port number error";
                if (_charonLogLevel >= CharonLogLevel.PublicCommands)
                    _rtuLogger35.AppendLine(LastErrorMessage, 2);
                return -1;
            }


            var masterPort = Children.First(pair => pair.Value == child).Key;
            if (GetActivePort() != masterPort)
            {
                var newMasterPort = SetActivePort(masterPort);
                if (newMasterPort != masterPort)
                    return -1;
            }

            var portOnChild = port - child.StartPortNumber + 1;
            var resultingPort = child.SetActivePort(portOnChild);
            if (portOnChild != resultingPort)
            {
                LastErrorMessage = child.LastErrorMessage;
                IsLastCommandSuccessful = child.IsLastCommandSuccessful;
                return resultingPort;
            }
            return port;
        }

        public bool DetachOtauFromPort(int fromOpticalPort)
        {
            var extPorts = GetExtentedPorts();
            if (extPorts == null)
                return false;
            if (!extPorts.ContainsKey(fromOpticalPort))
            {
                LastErrorMessage = "There is no such extended port. Nothing to do.";
                if (_charonLogLevel >= CharonLogLevel.PublicCommands)
                    _rtuLogger35.AppendLine(LastErrorMessage, 2);
                return false;
            }

            extPorts.Remove(fromOpticalPort);
            var content = DictionaryToContent(extPorts);
            SendWriteIniCommand(content);
            return IsLastCommandSuccessful;
        }

        public void RebootAdditionalMikrotik(string ip)
        {
            _rtuLogger35.AppendLine($"Reboot Mikrotik {ip} started...");
            Mikrotik mikrotik = new Mikrotik(ip, 5);
            if (!mikrotik.IsAvailable)
            {
                LastErrorMessage = $"Couldn't establish tcp connection with Mikrotik {ip}";
                _rtuLogger35.AppendLine(LastErrorMessage);
                IsLastCommandSuccessful = false;
                return ;
            }
            if (!mikrotik.Login(@"admin", ""))
            {
                LastErrorMessage = $@"Could not log in Mikrotik {ip}";
                IsLastCommandSuccessful = false;
                mikrotik.Close();
                return ;
            }
            mikrotik.Send(@"/system/reboot", true);
            _rtuLogger35.AppendLine("  reboot command sent");
            Thread.Sleep(TimeSpan.FromSeconds(20)); // reboot couldn't be less than 20 seconds
            for (int i = 0; i < 30; i++)
            {
                if (_charonLogLevel >= CharonLogLevel.BasicCommands )
                    _rtuLogger35.AppendLine("Check Mikrotik availability after reboot");
                Mikrotik mikrotik2 = new Mikrotik(ip, 1);
                if (mikrotik2.IsAvailable)
                {
                    IsLastCommandSuccessful = true;
                    return ;
                }
            }
            IsLastCommandSuccessful = false;
            LastErrorMessage = $"Couln't establish tcp connection with Mikrotik {ip} after reboot";
        }

        public bool AttachOtauToPort(NetAddress additionalOtauAddress, int toOpticalPort)
        {
            _rtuLogger35.AppendLine($"Attach {additionalOtauAddress.ToStringA()} to port {toOpticalPort} requested...");
            if (!ValidateAttachCommand(additionalOtauAddress, toOpticalPort))
                return false;
            var extPorts = GetExtentedPorts();
            if (extPorts == null) // read charon ini file error
            {
                LastErrorMessage = "Read charon ini file error";
                if (_charonLogLevel >= CharonLogLevel.PublicCommands)
                    _rtuLogger35.AppendLine(LastErrorMessage, 2);
                return false;
            }
            if (extPorts.ContainsKey(toOpticalPort))
            {
                LastErrorMessage = "This is extended port already. Denied.";
                if (_charonLogLevel >= CharonLogLevel.PublicCommands)
                    _rtuLogger35.AppendLine(LastErrorMessage, 2);
                return true;
            }
            extPorts.Add(toOpticalPort, additionalOtauAddress);
            var content = DictionaryToContent(extPorts);
            SendWriteIniCommand(content);
            return IsLastCommandSuccessful;
        }

        private bool ValidateAttachCommand(NetAddress additionalOtauAddress, int toOpticalPort)
        {
            if (toOpticalPort < 1 || toOpticalPort > OwnPortCount)
            {
                LastErrorMessage = $"Optical port number should be from 1 to {OwnPortCount}";
                if (_charonLogLevel >= CharonLogLevel.PublicCommands)
                    _rtuLogger35.AppendLine(LastErrorMessage, 2);
                IsLastCommandSuccessful = false;
                return false;
            }

            if (!additionalOtauAddress.HasValidTcpPort())
            {
                LastErrorMessage = "Tcp port number should be from 1 to 65355";
                if (_charonLogLevel >= CharonLogLevel.PublicCommands)
                    _rtuLogger35.AppendLine(LastErrorMessage, 2);
                IsLastCommandSuccessful = false;
                return false;
            }

            if (!additionalOtauAddress.HasValidIp4Address())
            {
                LastErrorMessage = "Invalid ip address";
                if (_charonLogLevel >= CharonLogLevel.PublicCommands)
                    _rtuLogger35.AppendLine(LastErrorMessage, 2);
                IsLastCommandSuccessful = false;
                return false;
            }

            return true;
        }

        public bool IsExtendedPortValidForMonitoring(ExtendedPort extendedPort)
        {
            var charon = GetCharon(extendedPort.NetAddress);
            if (charon == null)
            {
                _rtuLogger35.AppendLine($"Can't find address {extendedPort.NetAddress.ToStringA()}", 2);
                return false;
            }
            if (charon.Children.ContainsKey(extendedPort.Port))
            {
                _rtuLogger35.AppendLine($"Port {extendedPort.Port} is occupied by child charon", 2);
                return false;
            }
            if (charon.OwnPortCount < extendedPort.Port || extendedPort.Port < 1)
            {
                _rtuLogger35.AppendLine($"Port number for this otau should be from 1 to {charon.OwnPortCount}", 2);
                return false;
            }
            return true;
        }

        private Charon GetCharon(NetAddress netAddress)
        {
            if (NetAddress.Equals(netAddress))
                return this;
            foreach (var child in Children.Values)
            {
                var res = child.GetCharon(netAddress);
                if (res != null)
                    return res;
            }
            return null;
        }

        public string GetBopPortString(ExtendedPort extendedPort)
        {
            if (NetAddress.Equals(extendedPort.NetAddress))
                return extendedPort.Port.ToString();
            foreach (var pair in Children)
            {
                if (pair.Value.NetAddress.Equals(extendedPort.NetAddress))
                    return $"{pair.Key}:{extendedPort.Port}";
            }
            return $"Can't find port {extendedPort.ToStringA()}";
        }
    }
}