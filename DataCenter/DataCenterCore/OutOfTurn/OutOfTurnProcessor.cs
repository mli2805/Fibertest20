using System;
using System.Diagnostics;
using System.Threading;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph.RtuOccupy;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public class OutOfTurnProcessor
    {
        private readonly OutOfTurnData _outOfTurnData;
        private readonly RtuOccupations _rtuOccupations;
        private readonly ClientToRtuTransmitter _clientToRtuTransmitter;
        private readonly ClientToRtuVeexTransmitter _clientToRtuVeexTransmitter;
        private readonly IMyLog _logFile;
        private readonly string _trapSenderUser;

        public OutOfTurnProcessor(IniFile iniFile, OutOfTurnData outOfTurnData,  
            RtuOccupations rtuOccupations, ClientToRtuTransmitter clientToRtuTransmitter, ClientToRtuVeexTransmitter clientToRtuVeexTransmitter)
        {
            _outOfTurnData = outOfTurnData;
            _rtuOccupations = rtuOccupations;
            _clientToRtuTransmitter = clientToRtuTransmitter;
            _clientToRtuVeexTransmitter = clientToRtuVeexTransmitter;
            _logFile = new LogFile(iniFile, 20000);
            _logFile.AssignFile("out-of-turn.log");

            _trapSenderUser = rtuOccupations.ServerNameForTraps;
        }

        public void Start()
        {
            var thread = new Thread(Check) { IsBackground = true };
            thread.Start();
        }

        private async void Check()
        {
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            _logFile.AppendLine($"Out of turn measurements processor thread. Process {pid}, thread {tid}");

            try
            {
                while (true)
                {
                    var dto = _outOfTurnData.GetNextRequest(_logFile, _rtuOccupations, _trapSenderUser);
                    if (dto == null)
                    {
                        Thread.Sleep(7000);
                        _logFile.AppendLine($"Queue is empty.");
                        continue;
                    }

                    var unused =  dto.RtuMaker == RtuMaker.IIT
                        ? await _clientToRtuTransmitter.DoOutOfTurnPreciseMeasurementAsync(dto)
                        : await _clientToRtuVeexTransmitter.DoOutOfTurnPreciseMeasurementAsync(dto);
                }
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"{e.Message}");
            }
        }
    }
}