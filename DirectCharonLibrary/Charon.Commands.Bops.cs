using Iit.Fibertest.Utils35;

namespace Iit.Fibertest.DirectCharonLibrary
{
    public partial class Charon
    {
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
    }
}