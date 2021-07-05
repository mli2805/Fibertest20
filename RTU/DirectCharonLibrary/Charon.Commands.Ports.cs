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

        public CharonOperationResult SetExtendedActivePort(string serial, int port)
        {
            _rtuLogFile.AppendLine($"Toggling to port {port} on {serial}...");
            if (Serial == serial)
                return SetActivePortOnMainCharon(port);
            else
            {
                var bopCharon = GetBopCharonWithLogging(serial);
                if (bopCharon == null)
                    return CharonOperationResult.LogicalError;
                else
                {
                    var result = ToggleMasterCharonToBopIfNeeded(bopCharon);
                    return result == CharonOperationResult.Ok ? SetActivePortOnBopCharon(bopCharon, port) : result;
                }
            }
        }

        private CharonOperationResult SetActivePortOnMainCharon(int port)
        {
            var activePort = SetActivePort(port);
            if (activePort == port)
                return CharonOperationResult.Ok;

            _rtuLogFile.AppendLine("Toggling second attempt...");
            activePort = SetActivePort(port);
            if (activePort == port)
                return CharonOperationResult.Ok;

            LastErrorMessage = $"Can't toggle switch into {port} port";
            _rtuLogFile.AppendLine(LastErrorMessage, 2);
            return CharonOperationResult.MainOtauError;
        }

      
        public Charon GetBopCharonWithLogging(string serial)
        {
            var charon = Children.Values.FirstOrDefault(c => c.Serial == serial);
            if (charon == null)
            {
                LastErrorMessage = "There is no such optical switch";
                _rtuLogFile.AppendLine(LastErrorMessage, 2);
            }
            return charon;
        }

        private CharonOperationResult ToggleMasterCharonToBopIfNeeded(Charon charon)
        {
            var masterPort = Children.First(pair => pair.Value == charon).Key;
            return GetActivePort() != masterPort ? SetActivePortOnMainCharon(masterPort) : CharonOperationResult.Ok;
        }

        private CharonOperationResult SetActivePortOnBopCharon(Charon charon, int port)
        {
            var activePort = charon.SetActivePort(port);
            if (activePort == port)
                return CharonOperationResult.Ok;

            _rtuLogFile.AppendLine("Toggling second attempt...");
            activePort = charon.SetActivePort(port);
            if (activePort == port)
                return CharonOperationResult.Ok;

            LastErrorMessage = charon.LastErrorMessage;
            IsLastCommandSuccessful = charon.IsLastCommandSuccessful;
            _rtuLogFile.AppendLine(LastErrorMessage, 2);
            return CharonOperationResult.AdditionalOtauError;
        }
    }
}