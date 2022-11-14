using System;
using System.Diagnostics;
using System.Threading;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public class OutOfTurnProcessor
    {
        private readonly OutOfTurnData _outOfTurnData;
        private readonly Model _writeModel;
        private readonly ClientToRtuTransmitter _clientToRtuTransmitter;
        private readonly ClientToRtuVeexTransmitter _clientToRtuVeexTransmitter;
        private readonly IMyLog _logFile;


        public OutOfTurnProcessor(IniFile iniFile, OutOfTurnData outOfTurnData, Model writeModel,
            ClientToRtuTransmitter clientToRtuTransmitter, ClientToRtuVeexTransmitter clientToRtuVeexTransmitter)
        {
            _outOfTurnData = outOfTurnData;
            _writeModel = writeModel;
            _clientToRtuTransmitter = clientToRtuTransmitter;
            _clientToRtuVeexTransmitter = clientToRtuVeexTransmitter;
            _logFile = new LogFile(iniFile, 20000);
            _logFile.AssignFile("out-of-turn.log");
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
                    var dto = _outOfTurnData.GetNextRequest(_logFile);
                    if (dto == null)
                    {
                        Thread.Sleep(3000);
                        _logFile.AppendLine($"Queue {_outOfTurnData.QueueId.First6()} is empty.");
                        continue;
                    }

                    if (!_writeModel.TryGetRtu(dto.RtuId, out Rtu rtu)) return;

                    _outOfTurnData.SetRtuIsBusy(rtu.Id);

                    var unused = rtu.RtuMaker == RtuMaker.IIT
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