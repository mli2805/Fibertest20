using System.Linq;
using Iit.Fibertest.Dto;

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
            _rtuLogFile.AppendLine($"Toggling to port {port} on {charonAddress.ToStringA()}...");
            if (NetAddress.Equals(charonAddress))
            {
                var activePort = SetActivePort(port);
                _rtuLogFile.AppendLine("Toggled Ok.");
                return activePort == port ? CharonOperationResult.Ok : CharonOperationResult.MainOtauError;
            }

            var charon = Children.Values.FirstOrDefault(c => c.NetAddress.Equals(charonAddress));
            if (charon == null)
            {
                LastErrorMessage = "There is no such optical switch";
                if (_charonLogLevel >= CharonLogLevel.PublicCommands)
                    _rtuLogFile.AppendLine(LastErrorMessage, 2);
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
                        _rtuLogFile.AppendLine(LastErrorMessage, 2);
                    return CharonOperationResult.MainOtauError;
                }
            }

            var resultingPort = charon.SetActivePort(port);
            if (resultingPort != port)
            {
                LastErrorMessage = charon.LastErrorMessage;
                IsLastCommandSuccessful = charon.IsLastCommandSuccessful;
                if (_charonLogLevel >= CharonLogLevel.PublicCommands)
                    _rtuLogFile.AppendLine(LastErrorMessage, 2);
                return CharonOperationResult.AdditionalOtauError;
            }

            _rtuLogFile.AppendLine("Toggled Ok.");
            return CharonOperationResult.Ok;
        }
    }
}
