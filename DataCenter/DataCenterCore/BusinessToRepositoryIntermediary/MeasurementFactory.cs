using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
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
                ReturnCode = monitoringResultDto.ReturnCode,
                SorFileId = sorId,

                MeasurementTimestamp = monitoringResultDto.TimeStamp,
                EventRegistrationTimestamp = DateTime.Now,
                RtuId = monitoringResultDto.RtuId,
                TraceId = monitoringResultDto.PortWithTrace.TraceId,
                BaseRefType = monitoringResultDto.BaseRefType,
                TraceState = monitoringResultDto.TraceState,

                StatusChangedTimestamp = DateTime.Now,
                StatusChangedByUser = "system",
                Comment = "",

                Accidents = ExtractAccidents(monitoringResultDto.SorBytes, monitoringResultDto.PortWithTrace.TraceId)
            };
            EvaluateStatus(result);
            return result;
        }

        public AddRtuAccident CreateRtuProblemCommand(MonitoringResultDto monitoringResultDto)
        {
            return new AddRtuAccident()
            {
                IsMeasurementProblem = true,
                ReturnCode = monitoringResultDto.ReturnCode,

                EventRegistrationTimestamp = DateTime.Now,
                RtuId = monitoringResultDto.RtuId,
                TraceId = monitoringResultDto.PortWithTrace.TraceId,
                BaseRefType = monitoringResultDto.BaseRefType,

                Comment = "",
            };
        }

        private void EvaluateStatus(AddMeasurement cmd)
        {
            if (!IsEvent(cmd))
                cmd.EventStatus = EventStatus.JustMeasurementNotAnEvent;
            else if (cmd.TraceState == FiberState.Ok || cmd.BaseRefType == BaseRefType.Fast)
                cmd.EventStatus = EventStatus.EventButNotAnAccident;
            else cmd.EventStatus = EventStatus.Unprocessed;
        }

        private bool IsEvent(AddMeasurement cmd)
        {
            var previousMeasurementOnTrace = _writeModel.Measurements
                .Where(ev => ev.TraceId == cmd.TraceId).ToList()
                .LastOrDefault();
            if (previousMeasurementOnTrace == null)
            {
                _logFile.AppendLine($"First measurement on trace {cmd.TraceId.First6()} - event.");
                return true;
            }

            if (IsStateChanged(cmd, previousMeasurementOnTrace))
            {
                _logFile.AppendLine($"State of trace {cmd.TraceId.First6()} changed - event.");
                return true;
            }

            if (previousMeasurementOnTrace.BaseRefType == BaseRefType.Fast
                && previousMeasurementOnTrace.EventStatus >
                EventStatus.JustMeasurementNotAnEvent // fast measurement could be made 
                                                      // when monitoring mode is turned to Automatic 
                                                      // or it could be made by schedule
                                                      // but we are interested only in Events
                && cmd.BaseRefType != BaseRefType.Fast // Precise or Additional
                && cmd.TraceState != FiberState.Ok)
            {
                _logFile.AppendLine(
                    $"Confirmation of accident on trace {cmd.TraceId.First6()} - event.");
                return true;
            }

            return false;
        }

        private bool IsStateChanged(AddMeasurement current, Measurement previous)
        {
            if (current.TraceState != previous.TraceState)
                return true;
            if (current.TraceState == FiberState.Ok || current.TraceState == FiberState.NoFiber)
                return false;
            if (current.Accidents.Count != previous.Accidents.Count)
                return true;
            for (int i = 0; i < current.Accidents.Count; i++)
            {
                if (!current.Accidents[i].IsTheSame(previous.Accidents[i])) return true;
            }

            return false;
        }
        
        private List<AccidentOnTraceV2> ExtractAccidents(byte[] sorBytes, Guid traceId)
        {
            var sorData = SorData.FromBytes(sorBytes);
            var accidents = _globalScope.Resolve<AccidentsFromSorExtractor>().GetAccidents(sorData, traceId, false);
            return accidents;
        }
    }
}