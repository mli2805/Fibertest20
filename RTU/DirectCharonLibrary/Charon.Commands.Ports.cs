﻿using System.Linq;
using Dto;
using Iit.Fibertest.UtilsLib;

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

        public bool IsExtendedPortValidForMonitoring(ExtendedPort extendedPort)
        {
            var charon = GetCharon(extendedPort.NetAddress);
            if (charon == null)
            {
                _rtuLogFile.AppendLine($"Can't find address {extendedPort.NetAddress.ToStringA()}", 2);
                return false;
            }
            if (charon.Children.ContainsKey(extendedPort.OpticalPort))
            {
                _rtuLogFile.AppendLine($"Port {extendedPort.OpticalPort} is occupied by child charon", 2);
                return false;
            }
            if (charon.OwnPortCount < extendedPort.OpticalPort || extendedPort.OpticalPort < 1)
            {
                _rtuLogFile.AppendLine($"Port number for this otau should be from 1 to {charon.OwnPortCount}", 2);
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
                return extendedPort.OpticalPort.ToString();
            foreach (var pair in Children)
            {
                if (pair.Value.NetAddress.Equals(extendedPort.NetAddress))
                    return $"{pair.Key}:{extendedPort.OpticalPort}";
            }
            return $"Can't find port {extendedPort.ToStringA()}";
        }
    }
}
