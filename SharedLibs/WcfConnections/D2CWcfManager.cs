using System;
using System.Collections.Generic;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.WcfConnections
{
    public class D2CWcfManager
    {
        private readonly List<DoubleAddress> _addresses;
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;

        public D2CWcfManager(List<DoubleAddress> addresses, IniFile iniFile, IMyLog logFile)
        {
            _addresses = addresses;
            _iniFile = iniFile;
            _logFile = logFile;
        }

    
        public bool ProcessRtuCurrentMonitoringStep(KnowRtuCurrentMonitoringStepDto dto)
        {
            foreach (var clientAddress in _addresses)
            {
                var wcfConnection = new WcfFactory(clientAddress, _iniFile, _logFile).CreateClientConnection();
                if (wcfConnection == null)
                    continue;

                try
                {
                    wcfConnection.ProcessRtuCurrentMonitoringStep(dto);
//                    _logFile.AppendLine($"Transfered RTU {dto.RtuId.First6()} monitoring step to client {clientAddress.Main.ToStringA()}");
                }
                catch (Exception e)
                {
                    _logFile.AppendLine(e.Message);
                }
            }
            return true;
        }

    
    }
}