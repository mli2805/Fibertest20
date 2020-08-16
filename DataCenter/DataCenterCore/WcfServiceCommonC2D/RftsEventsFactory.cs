﻿using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.IitOtdrLibrary;
using Optixsoft.SorExaminer.OtdrDataFormat;
using Optixsoft.SorExaminer.OtdrDataFormat.Structures;

namespace Iit.Fibertest.DataCenterCore
{
    public static class RftsEventsFactory
    {
        public static RftsEventsDto Create(byte[] sorBytes)
        {
            var rftsEventsDto = new RftsEventsDto { ReturnCode = ReturnCode.Ok };
            var sorData = SorData.FromBytes(sorBytes);

            rftsEventsDto.IsNoFiber = sorData.RftsEvents.MonitoringResult == (int)ComparisonReturns.NoFiber;
            if (rftsEventsDto.IsNoFiber) return rftsEventsDto;

            rftsEventsDto.LevelArray = CreateLevelArray(sorData).ToArray();
            rftsEventsDto.Footer = new RftsEventsFooterDto(){Orl = sorData.KeyEvents.OpticalReturnLoss};
            return rftsEventsDto;
        }

        private static IEnumerable<RftsLevelDto> CreateLevelArray(OtdrDataKnownBlocks sorData)
        {
            var rftsEventsBlocks = sorData.GetRftsEventsBlockForEveryLevel().ToList();
            var rftsParameters = sorData.RftsParameters;
            for (int i = 0; i < rftsParameters.LevelsCount; i++)
            {
                var level = rftsParameters.Levels[i];
                if (level.IsEnabled)
                {
                    var rftsLevelDto = CreateLevel(sorData,
                        rftsEventsBlocks.FirstOrDefault(b => b.LevelName == level.LevelName), level);

                    yield return rftsLevelDto;
                }
            }
        }

        private static RftsLevelDto CreateLevel(OtdrDataKnownBlocks sorData, RftsEventsBlock eventBlock, RftsLevel level)
        {
            var rftsLevelDto = new RftsLevelDto() { Title = level.LevelName.ToSid() };
            rftsLevelDto.EventArray = CreateEventArray(sorData, eventBlock, level).ToArray();
            var firstFailedEvent = rftsLevelDto.EventArray.FirstOrDefault(e => e.IsNew || e.IsFailed);
            if (firstFailedEvent != null)
            {
                rftsLevelDto.IsFailed = true;
                rftsLevelDto.FirstProblemLocation = firstFailedEvent.DistanceKm;
            }
            rftsLevelDto.TotalFiberLoss = CreateTotalFiberLossDto(sorData, level, eventBlock);
            return rftsLevelDto;
        }

        private static IEnumerable<RftsEventDto> CreateEventArray(OtdrDataKnownBlocks sorData, RftsEventsBlock rftsEventsBlock, RftsLevel level)
        {
            for (int i = 0; i < rftsEventsBlock.EventsCount; i++)
            {
                var rftsEventDto = new RftsEventDto() { Ordinal = i };

                // Common information
                var landmark = sorData.LinkParameters.LandmarkBlocks.FirstOrDefault(b => b.RelatedEventNumber == i + 1);
                if (landmark != null)
                {
                    rftsEventDto.LandmarkTitle = landmark.Comment;
                    rftsEventDto.LandmarkType = landmark.Code.ForTable();
                }

                rftsEventDto.DistanceKm = $@"{sorData.OwtToLenKm(sorData.KeyEvents.KeyEvents[i].EventPropagationTime):0.00000}";
                if ((rftsEventsBlock.Events[i].EventTypes & RftsEventTypes.IsNew) != 0)
                    rftsEventDto.IsNew = true;
                rftsEventDto.Enabled = rftsEventsBlock.Events[i].EventTypes.ForEnabledInTable();
                rftsEventDto.EventType = sorData.KeyEvents.KeyEvents[i].EventCode.EventCodeForTable();


                // Current measurement
                rftsEventDto.reflectanceCoeff = sorData.KeyEvents.KeyEvents[i].EventReflectance.ToString(CultureInfo.CurrentCulture);
                if (i != 0)
                {
                    var eventLoss = sorData.KeyEvents.KeyEvents[i].EventLoss;
                    var endOfFiberThreshold = sorData.FixedParameters.EndOfFiberThreshold;
                    rftsEventDto.attenuationInClosure = eventLoss > endOfFiberThreshold ? $@">{endOfFiberThreshold:0.000}" : $@"{eventLoss:0.000}";
                    var attenuationCoeffToDbKm = sorData.KeyEvents.KeyEvents[i].LeadInFiberAttenuationCoefficient /
                                                 sorData.GetOwtToKmCoeff();
                    rftsEventDto.attenuationCoeff = $@"{attenuationCoeffToDbKm: 0.000}";
                }

                // Monitoring threshold
                if ((rftsEventsBlock.Events[i].EventTypes & RftsEventTypes.IsNew) == 0)
                {
                    var threshold = level.ThresholdSets[i];
                    rftsEventDto.reflectanceCoeffThreshold = threshold.ReflectanceThreshold.Convert();
                    rftsEventDto.attenuationInClosureThreshold = threshold.AttenuationThreshold.Convert();
                    rftsEventDto.attenuationCoeffThreshold = threshold.AttenuationCoefThreshold.Convert();
                }

                // Deviations
                if ((rftsEventsBlock.Events[i].EventTypes & RftsEventTypes.IsFiberBreak) != 0)
                {
                    rftsEventDto.IsFailed = true;
                    rftsEventDto.DamageType = @"B";
                }

                rftsEventDto.reflectanceCoeffDeviation
                    = ForDeviationInTable(rftsEventDto, rftsEventsBlock.Events[i].ReflectanceThreshold, @"R");

                if (i < rftsEventsBlock.EventsCount - 1)
                    rftsEventDto.attenuationInClosureDeviation
                        = ForDeviationInTable(rftsEventDto, rftsEventsBlock.Events[i].AttenuationThreshold, @"L");

                rftsEventDto.attenuationCoeffDeviation
                    = ForDeviationInTable(rftsEventDto, rftsEventsBlock.Events[i].AttenuationCoefThreshold, @"C");

                rftsEventDto.State = rftsEventsBlock.Events[i].EventTypes.ForStateInTable(rftsEventDto.IsFailed);
                yield return rftsEventDto;
            }
        }
        private static string ForDeviationInTable(RftsEventDto rftsEventDto, ShortDeviation deviation, string letter)
        {
            var formattedValue = $@"{(short)deviation.Deviation / 1000.0: 0.000}";
            if ((deviation.Type & ShortDeviationTypes.IsExceeded) != 0)
            {
                formattedValue += $@" ( {letter} ) ";
                rftsEventDto.IsFailed = true;
                rftsEventDto.DamageType += $@" {letter}";
            }
            return formattedValue;
        }

        private static MonitoringThreshold Convert(this ShortThreshold threshold)
        {
            return new MonitoringThreshold
            {
                Value = threshold.IsAbsolute ? threshold.AbsoluteThreshold : threshold.RelativeThreshold,
                IsAbsolute = threshold.IsAbsolute
            };
        }


        private static TotalFiberLossDto CreateTotalFiberLossDto(OtdrDataKnownBlocks sorData, RftsLevel rftsLevel, RftsEventsBlock rftsEventsBlock)
        {
            return new TotalFiberLossDto()
            {
                Value = sorData.KeyEvents.EndToEndLoss,
                Threshold = rftsLevel.EELT.Convert(),
                Deviation = (short)rftsEventsBlock.EELD.Deviation / 1000.0,
                IsPassed = (rftsEventsBlock.EELD.Type & ShortDeviationTypes.IsExceeded) == 0,
            };
        }
    }
}
