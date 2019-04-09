using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public class MeasurementCleaner
    {
        private readonly SorFileRepository _sorFileRepository;
        private readonly Model _writeModel;
        private readonly IEventStoreInitializer _eventStoreInitializer;

        public MeasurementCleaner(SorFileRepository sorFileRepository, Model writeModel,
            IEventStoreInitializer eventStoreInitializer)
        {
            _sorFileRepository = sorFileRepository;
            _writeModel = writeModel;
            _eventStoreInitializer = eventStoreInitializer;
        }

        public async Task<int> ClearSor(StartDbOptimazation cmd)
        {
            if (!cmd.IsMeasurementsNotEvents && !cmd.IsOpticalEvents) return 0;

            var records = await _sorFileRepository.RemoveManySorAsync(
                (from meas in _writeModel.Measurements.Where(m => m.EventRegistrationTimestamp.Date <= cmd.UpTo.Date) 
                    where meas.EventStatus == EventStatus.JustMeasurementNotAnEvent && cmd.IsMeasurementsNotEvents 
                          || meas.EventStatus != EventStatus.JustMeasurementNotAnEvent && cmd.IsOpticalEvents 
                    select meas.SorFileId).ToArray());

            var unused = _eventStoreInitializer.OptimizeSorFilesTable();
            return records;
        }
    }

    public class MeasurementFactory
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IMyLog _logFile;
        private readonly Model _writeModel;

        public MeasurementFactory(ILifetimeScope globalScope, IMyLog logFile, Model writeModel)
        {
            _globalScope = globalScope;
            _logFile = logFile;
            _writeModel = writeModel;
        }

        // For Migrator only!
        public AddMeasurement CreateCommand(AddMeasurementFromOldBase dto, int sorId)
        {
            var result = new AddMeasurement
            {
                SorFileId = sorId,

                MeasurementTimestamp = dto.MeasurementTimestamp,
                EventRegistrationTimestamp = DateTime.Now,
                RtuId = dto.RtuId,
                TraceId = dto.TraceId,
                BaseRefType = dto.BaseRefType,
                TraceState = dto.TraceState,

                EventStatus = EventStatus.JustMeasurementNotAnEvent,
                StatusChangedTimestamp = DateTime.Now,
                StatusChangedByUser = "Migrator",
                Comment = "",

                //  Accidents = ExtractAccidents(dto.SorBytes, dto.TraceId)
            };
            return result;
        }

        public AddMeasurement CreateCommand(MonitoringResultDto monitoringResultDto, int sorId)
        {
            var result = new AddMeasurement
            {
                SorFileId = sorId,

                MeasurementTimestamp = monitoringResultDto.TimeStamp,
                EventRegistrationTimestamp = DateTime.Now,
                RtuId = monitoringResultDto.RtuId,
                TraceId = monitoringResultDto.PortWithTrace.TraceId,
                BaseRefType = monitoringResultDto.BaseRefType,
                TraceState = monitoringResultDto.TraceState,

                EventStatus = EvaluateStatus(monitoringResultDto),
                StatusChangedTimestamp = DateTime.Now,
                StatusChangedByUser = "system",
                Comment = "",

                Accidents = ExtractAccidents(monitoringResultDto.SorBytes, monitoringResultDto.PortWithTrace.TraceId)
            };
            return result;
        }


        private bool IsEvent(MonitoringResultDto result)
        {
            var previousMeasurementOnTrace = _writeModel.Measurements
                .Where(ev => ev.TraceId == result.PortWithTrace.TraceId).ToList()
                .LastOrDefault();
            if (previousMeasurementOnTrace == null)
            {
                _logFile.AppendLine($"First measurement on trace {result.PortWithTrace.TraceId.First6()} - event.");
                return true;
            }

            if (previousMeasurementOnTrace.TraceState != result.TraceState)
            {
                _logFile.AppendLine($"State of trace {result.PortWithTrace.TraceId.First6()} changed - event.");
                return true;
            }

            if (previousMeasurementOnTrace.BaseRefType == BaseRefType.Fast
                && previousMeasurementOnTrace.EventStatus >
                EventStatus.JustMeasurementNotAnEvent // fast measurement could be made 
                                                      // when monitoring mode is turned to Automatic 
                                                      // or it could be made by schedule
                                                      // but we are interested only in Events
                && result.BaseRefType != BaseRefType.Fast // Precise or Additional
                && result.TraceState != FiberState.Ok)
            {
                _logFile.AppendLine(
                    $"Confirmation of accident on trace {result.PortWithTrace.TraceId.First6()} - event.");
                return true;
            }

            return false;
        }

        private EventStatus EvaluateStatus(MonitoringResultDto result)
        {
            if (!IsEvent(result))
                return EventStatus.JustMeasurementNotAnEvent;
            if (result.TraceState == FiberState.Ok || result.BaseRefType == BaseRefType.Fast)
                return EventStatus.EventButNotAnAccident;
            return EventStatus.Unprocessed;
        }

        private List<AccidentOnTraceV2> ExtractAccidents(byte[] sorBytes, Guid traceId)
        {
            var sorData = SorData.FromBytes(sorBytes);
            var accidents = _globalScope.Resolve<AccidentsFromSorExtractor>().GetAccidents(sorData, traceId, false);
            return accidents;
        }
    }
}