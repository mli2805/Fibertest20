using Iit.Fibertest.Dto;

namespace Iit.Fibertest.DirectCharonLibrary
{
    public partial class Charon
    {
        public bool DetachOtauFromPort(int fromOpticalPort)
        {
            _rtuLogFile.AppendLine($"Detach from port {fromOpticalPort} requested...");
            var extPorts = GetExtendedPorts();
            if (extPorts == null)
                return false;
            if (LastAnswer.Substring(0, 15) == "ERROR_COMMAND\r\n")
            {
                return true;
            }
            if (!extPorts.ContainsKey(fromOpticalPort))
            {
                LastErrorMessage = "There is no such extended port. Nothing to do.";
                _rtuLogFile.AppendLine(LastErrorMessage, 2);
                return true;
            }

            extPorts.Remove(fromOpticalPort);
            var content = DictionaryToContent(extPorts);
            SendWriteIniCommand(content);

            if (IsLastCommandSuccessful)
            {
                var child = Children[fromOpticalPort];
                FullPortCount -= child.FullPortCount;
                Children.Remove(fromOpticalPort);
            }
            return IsLastCommandSuccessful;
        }

        public Charon AttachOtauToPort(NetAddress additionalOtauAddress, int toOpticalPort)
        {
            _rtuLogFile.AppendLine($"Attach {additionalOtauAddress.ToStringA()} to port {toOpticalPort} requested...");
            if (!ValidateAttachCommand(additionalOtauAddress, toOpticalPort))
                return null;
            var extPorts = GetExtendedPorts();
            if (extPorts == null) // read charon ini file error
            {
                return null;
            }
            if (LastAnswer.Substring(0, 15) == "ERROR_COMMAND\r\n")
            {
                return null;
            }
            if (extPorts.ContainsKey(toOpticalPort))
            {
                LastErrorMessage = "This is extended port already. Denied.";
                _rtuLogFile.AppendLine(LastErrorMessage, 2);
                return null;
            }

            _rtuLogFile.AppendLine($"Check connection with OTAU {additionalOtauAddress.ToStringA()}");
            var child = new Charon(additionalOtauAddress, false, _iniFile35, _rtuLogFile);
            if (child.InitializeOtauRecursively() != null)
            {
                return null;
            }

            extPorts.Add(toOpticalPort, additionalOtauAddress);
            var content = DictionaryToContent(extPorts);
            SendWriteIniCommand(content);

            if (IsLastCommandSuccessful)
            {
                Children.Add(toOpticalPort, child);
                FullPortCount += child.FullPortCount;
            }

            return child;
        }

        private bool ValidateAttachCommand(NetAddress additionalOtauAddress, int toOpticalPort)
        {
            if (toOpticalPort < 1 || toOpticalPort > OwnPortCount)
            {
                LastErrorMessage = $"Optical port number should be from 1 to {OwnPortCount}";
                _rtuLogFile.AppendLine(LastErrorMessage, 2);
                IsLastCommandSuccessful = false;
                return false;
            }

            if (!additionalOtauAddress.HasValidTcpPort())
            {
                LastErrorMessage = "Tcp port number should be from 1 to 65355";
                _rtuLogFile.AppendLine(LastErrorMessage, 2);
                IsLastCommandSuccessful = false;
                return false;
            }

            if (!additionalOtauAddress.HasValidIp4Address())
            {
                LastErrorMessage = "Invalid ip address";
                _rtuLogFile.AppendLine(LastErrorMessage, 2);
                IsLastCommandSuccessful = false;
                return false;
            }

            return true;
        }
    }
}