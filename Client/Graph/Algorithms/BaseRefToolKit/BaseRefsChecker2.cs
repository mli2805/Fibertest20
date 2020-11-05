using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Optixsoft.SorExaminer.OtdrDataFormat;

namespace Iit.Fibertest.Graph
{
    public class BaseRefsChecker2
    {
        private readonly Model _writeModel;
        private readonly BaseRefLandmarksTool _baseRefLandmarksTool;

        public BaseRefsChecker2(Model writeModel, BaseRefLandmarksTool baseRefLandmarksTool)
        {
            _writeModel = writeModel;
            _baseRefLandmarksTool = baseRefLandmarksTool;
        }

        public BaseRefAssignedDto AreBaseRefsAcceptable(List<BaseRefDto> baseRefsDto, Trace trace)
        {
            var assignmentFailed = new BaseRefAssignedDto() { ReturnCode = ReturnCode.BaseRefAssignmentFailed };
            try
            {
                var rtu = _writeModel.Rtus.FirstOrDefault(r => r.Id == trace.RtuId);
                if (rtu == null) return assignmentFailed;

                foreach (var baseRefDto in baseRefsDto)
                {
                    if (baseRefDto.Id == Guid.Empty) continue; //

                    assignmentFailed.BaseRefType = baseRefDto.BaseRefType;
                    var message = SorData.TryGetFromBytes(baseRefDto.SorBytes, out var otdrKnownBlocks);
                    if (message != "")
                    {
                        assignmentFailed.ErrorMessage = message;
                        return assignmentFailed;
                    }

                    var checkParamsResult = CheckMeasParams(otdrKnownBlocks, rtu.AcceptableMeasParams);
                    if (checkParamsResult.ReturnCode != ReturnCode.BaseRefAssignedSuccessfully)
                    {
                        checkParamsResult.BaseRefType = baseRefDto.BaseRefType;
                        return checkParamsResult;
                    }

                    if (otdrKnownBlocks.RftsParameters.LevelsCount == 0)
                        return new BaseRefAssignedDto()
                        {
                            BaseRefType = baseRefDto.BaseRefType,
                            ReturnCode = ReturnCode.BaseRefAssignmentNoThresholds
                        };

                    var result = CheckLandmarks(trace, otdrKnownBlocks);
                    if (result.ReturnCode != ReturnCode.BaseRefAssignedSuccessfully)
                    {
                        result.BaseRefType = baseRefDto.BaseRefType;
                        return result;
                    }

                    baseRefDto.SorBytes = _baseRefLandmarksTool.ApplyTraceToBaseRef(otdrKnownBlocks, trace,
                        result.Landmarks == result.Equipments);
                    baseRefDto.Duration = TimeSpan.FromSeconds((int) otdrKnownBlocks.FixedParameters.AveragingTime);
                }

                return new BaseRefAssignedDto() { ReturnCode = ReturnCode.BaseRefAssignedSuccessfully };

            }
            catch (Exception e)
            {
                assignmentFailed.ErrorMessage = e.Message;
                return assignmentFailed;
            }
        }

        private BaseRefAssignedDto CheckMeasParams(OtdrDataKnownBlocks otdrKnownBlocks,
            TreeOfAcceptableMeasParams acceptableMeasParams)
        {
            var checkResult = IsWaveLengthAcceptable(otdrKnownBlocks, acceptableMeasParams);
            if (checkResult.ReturnCode != ReturnCode.BaseRefAssignedSuccessfully)
                return checkResult;
            //TODO check other parameters
            return checkResult;
        }

        private BaseRefAssignedDto IsWaveLengthAcceptable(OtdrDataKnownBlocks otdrKnownBlocks,
            TreeOfAcceptableMeasParams acceptableMeasParams)
        {
            var waveLength = otdrKnownBlocks.GeneralParameters.NominalWavelength.ToString(CultureInfo.InvariantCulture);
            foreach (var unit in acceptableMeasParams.Units.Keys) // "SM NNNN" or "SMNNNN"
            {
                if (unit.Contains(waveLength))
                    return new BaseRefAssignedDto(){ReturnCode = ReturnCode.BaseRefAssignedSuccessfully};
            }

            return new BaseRefAssignedDto(){ReturnCode = ReturnCode.BaseRefAssignmentParamNotAcceptable, WaveLength = waveLength};
        }

        private BaseRefAssignedDto CheckLandmarks(Trace trace, OtdrDataKnownBlocks otdrDataKnownBlocks)
        {
            var landmarks = otdrDataKnownBlocks.LinkParameters.LandmarkBlocks;
            if (landmarks[0].RelatedEventNumber == 0 || landmarks[landmarks.Length - 1].RelatedEventNumber == 0)
                return new BaseRefAssignedDto() { ReturnCode = ReturnCode.BaseRefAssignmentEdgeLandmarksWrong };

            return IsBaseRefLandmarkCountMatched(trace, otdrDataKnownBlocks);
        }

        private BaseRefAssignedDto IsBaseRefLandmarkCountMatched(Trace trace, OtdrDataKnownBlocks otdrKnownBlocks)
        {
            var landmarkCount = otdrKnownBlocks.LinkParameters.LandmarksCount;

            var equipments = _writeModel.GetTraceEquipments(trace).ToList(); // without RTU
            var nodesCount = equipments.Count(eq => eq.Type > EquipmentType.AdjustmentPoint) + 1; // without adjustment points
            var equipmentsCount = equipments.Count(eq => eq.Type > EquipmentType.CableReserve) + 1; // sic! in this case CableReserve is not an equipment

            var result = new BaseRefAssignedDto()
            {
                Landmarks = landmarkCount,
                Equipments = equipmentsCount,
                Nodes = nodesCount
            };
            result.ReturnCode = landmarkCount == nodesCount || landmarkCount == equipmentsCount
                ? ReturnCode.BaseRefAssignedSuccessfully
                : ReturnCode.BaseRefAssignmentLandmarkCountWrong;
            return result;
        }
    }
}