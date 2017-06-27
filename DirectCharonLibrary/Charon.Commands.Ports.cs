using System.Linq;
using Iit.Fibertest.Utils35;

namespace Iit.Fibertest.DirectCharonLibrary
{
    public partial class Charon
    {
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

        public CharonOperationResult SetExtendedActivePort(NetAddress charonAddress, int port)
        {
            _rtuLogger35.AppendLine($"Toggling to port {port} on {charonAddress.ToStringA()}...");
            if (NetAddress.Equals(charonAddress))
            {
                var activePort = SetActivePort(port);
                _rtuLogger35.AppendLine("Toggled Ok.");
                return activePort == port ? CharonOperationResult.Ok : CharonOperationResult.MainOtauError;
            }

            var charon = Children.Values.FirstOrDefault(c => c.NetAddress.Equals(charonAddress));
            if (charon == null)
            {
                LastErrorMessage = "There is no such optical switch";
                if (_charonLogLevel >= CharonLogLevel.PublicCommands)
                    _rtuLogger35.AppendLine(LastErrorMessage, 2);
                return CharonOperationResult.LogicalError;
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
                    return CharonOperationResult.MainOtauError;
                }
            }

            var resultingPort = charon.SetActivePort(port);
            if (resultingPort != port)
            {
                LastErrorMessage = charon.LastErrorMessage;
                IsLastCommandSuccessful = charon.IsLastCommandSuccessful;
                if (_charonLogLevel >= CharonLogLevel.PublicCommands)
                    _rtuLogger35.AppendLine(LastErrorMessage, 2);
                return CharonOperationResult.AdditionalOtauError;
            }

            _rtuLogger35.AppendLine("Toggled Ok.");
            return CharonOperationResult.Ok;
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
