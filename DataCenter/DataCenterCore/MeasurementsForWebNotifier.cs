using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public class MeasurementsForWebNotifier
    {
        private readonly IMyLog _logFile;
        private readonly IFtSignalRClient _ftSignalRClient;
        private readonly ConcurrentQueue<ClientMeasurementResultDto> _measurements = new ConcurrentQueue<ClientMeasurementResultDto>();
        private readonly ConcurrentDictionary<Guid, byte[]> _measDict = new ConcurrentDictionary<Guid, byte[]>();

        public MeasurementsForWebNotifier(IMyLog logFile, IFtSignalRClient ftSignalRClient)
        {
            _logFile = logFile;
            _ftSignalRClient = ftSignalRClient;
        }

        public void Push(ClientMeasurementResultDto measurement)
        {
            _measurements.Enqueue(measurement);
         }

        public void Start()
        {
            var thread = new Thread(Check) { IsBackground = true };
            thread.Start();
        }

        // ReSharper disable once FunctionNeverReturns 
        private async void Check()
        {
            while (true)
            {
                await Tick();
                Thread.Sleep(15000);
            }
        }

        private async Task Tick()
        {
            if (_measurements.TryDequeue(out ClientMeasurementResultDto sorBytesDto))
            {
                sorBytesDto.Id = Guid.NewGuid();
                _measDict.TryAdd(sorBytesDto.Id, sorBytesDto.SorBytes);
                sorBytesDto.SorBytes = null;
                _logFile.AppendLine($"measurement result saved with id {sorBytesDto.Id}");
                await _ftSignalRClient.SendToOne(sorBytesDto.ConnectionId, "ClientMeasurementDone", sorBytesDto.ToCamelCaseJson());
            }
        }

        public byte[] Extract(Guid id)
        {
            if (!_measDict.ContainsKey(id)) return null;
            return _measDict.TryRemove(id, out byte[] result) ? result : null;
        }
    }
}