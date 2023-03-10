using System;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Optixsoft.SorExaminer.OtdrDataFormat;

namespace Iit.Fibertest.Graph
{
    public class BaseRefLandmarksTool
    {
        private static readonly double LinK = 1.02;

        private readonly Model _readModel;
        private readonly TraceModelBuilder _traceModelBuilder;

        public BaseRefLandmarksTool(Model readModel, TraceModelBuilder traceModelBuilder)
        {
            _readModel = readModel;
            _traceModelBuilder = traceModelBuilder;
        }

        public void AugmentFastBaseRefSentByMigrator(Guid traceId, BaseRefDto baseRefDto)
        {
            var trace = _readModel.Traces.First(t => t.TraceId == traceId);
            var message = SorData.TryGetFromBytes(baseRefDto.SorBytes, out var otdrKnownBlocks);
            if (message != "") return;
            ApplyTraceToBaseRef(otdrKnownBlocks, trace, otdrKnownBlocks.LinkParameters.LandmarkBlocks.Length < trace.NodeIds.Count);
            baseRefDto.SorBytes = otdrKnownBlocks.ToBytes();
        }

        public void ApplyTraceToBaseRef(OtdrDataKnownBlocks otdrKnownBlocks, Trace trace,
            bool needToInsertLandmarksForEmptyNodes)
        {
            var traceModel = _readModel.GetTraceComponentsByIds(trace);
            var modelWithoutAdjustmentPoint = _traceModelBuilder.GetTraceModelWithoutAdjustmentPoints(traceModel);
            if (needToInsertLandmarksForEmptyNodes)
                InsertLandmarks(otdrKnownBlocks, modelWithoutAdjustmentPoint);

            ReCalculateLandmarksLocations(otdrKnownBlocks, modelWithoutAdjustmentPoint);

            AddNamesAndTypesForLandmarks(otdrKnownBlocks, modelWithoutAdjustmentPoint);
        }

        public void ReCalculateLandmarksLocations(OtdrDataKnownBlocks sorData, TraceModelForBaseRef model)
        {
            var landmarks = sorData.LinkParameters.LandmarkBlocks;
            var distancesMm = new int[landmarks.Length - 1];


            var leftLandmarkIndex = 0;

            while (leftLandmarkIndex < landmarks.Length - 1)
            {
                var rightLandmarkIndex = GetIndexOfLastLandmarkOfFixedSection(model, leftLandmarkIndex);

                var ratio = GetRatioForFixedSection(sorData, model, leftLandmarkIndex, rightLandmarkIndex);

                for (int i = leftLandmarkIndex; i < rightLandmarkIndex; i++)
                {
                    int pos;
                    if (model.FiberArray[i].UserInputedLength > 0)
                    {
                        pos = (int)Math.Round(model.FiberArray[i].UserInputedLength * 1000 * LinK);
                    }
                    else
                    {
                        pos = (int)Math.Round(model.DistancesMm[i] * ratio);
                    }
                    pos += (int)Math.Round(model.EquipArray[i].CableReserveRight * LinK);
                    pos += (int)Math.Round(model.EquipArray[i + 1].CableReserveLeft * LinK);
                    distancesMm[i] = pos;
                }

                leftLandmarkIndex = rightLandmarkIndex;
            }

            for (int i = 0; i < distancesMm.Length; i++)
            {
                landmarks[i + 1].Location = landmarks[i].Location + sorData.GetOwtFromMm(distancesMm[i]);
            }
        }

        private void InsertLandmarks(OtdrDataKnownBlocks sorData, TraceModelForBaseRef model)
        {
            var newLandmarks = new Optixsoft.SorExaminer.OtdrDataFormat.Structures.Landmark[model.EquipArray.Length];

            var oldLandmarkIndex = 0;
            for (var i = 0; i < model.EquipArray.Length; i++)
            {
                if (model.EquipArray[i].Type > EquipmentType.CableReserve)
                {
                    newLandmarks[i] = sorData.LinkParameters.LandmarkBlocks[oldLandmarkIndex];
                    oldLandmarkIndex++;
                }
                else
                    newLandmarks[i] = new Optixsoft.SorExaminer.OtdrDataFormat.Structures.Landmark() { Code = LandmarkCode.Manhole };
            }

            sorData.LinkParameters.LandmarkBlocks = newLandmarks;
            sorData.LinkParameters.LandmarksCount = (short)newLandmarks.Length;
        }

        public void AddNamesAndTypesForLandmarks(OtdrDataKnownBlocks sorData, TraceModelForBaseRef model)
        {
            var landmarks = sorData.LinkParameters.LandmarkBlocks;

            for (int i = 0; i < landmarks.Length; i++)
            {
                var landmarkTitle = model.NodeArray[i].Title;
                if (i != 0 && !string.IsNullOrEmpty(model.EquipArray[i].Title))
                    landmarkTitle += $@" / {model.EquipArray[i].Title}";

                landmarks[i].Comment = landmarkTitle; // utf8, Reflect can now read it
                landmarks[i].Code = model.EquipArray[i].Type.ToLandmarkCode();
                landmarks[i].GpsLatitude = GisLabCalculator.GpsInSorFormat(model.NodeArray[i].Position.Lat);
                landmarks[i].GpsLongitude = GisLabCalculator.GpsInSorFormat(model.NodeArray[i].Position.Lng);
            }
        }

        private int GetIndexOfLastLandmarkOfFixedSection(TraceModelForBaseRef model, int firstLandmarkIndex)
        {
            var endIndex = firstLandmarkIndex + 1;
            while (model.EquipArray[endIndex].Type <= EquipmentType.CableReserve)
                endIndex++;
            return endIndex;
        }

        private double GetRatioForFixedSection(OtdrDataKnownBlocks sorData, TraceModelForBaseRef model,
            int leftRealEquipmentIndex, int rightRealEquipmentIndex)
        {
            var onGraph = GetNotUserInputDistancesFromGraph(model, leftRealEquipmentIndex, rightRealEquipmentIndex);
            var onBaseRef =
                GetNotUserInputDistancesFromRefMm(sorData, model, leftRealEquipmentIndex, rightRealEquipmentIndex);
            return onBaseRef / onGraph;
        }

        private int GetNotUserInputDistancesFromGraph(TraceModelForBaseRef model,
            int leftEquipmentIndex, int rightEquipmentIndex)
        {
            int result = 0;
            for (int i = leftEquipmentIndex; i < rightEquipmentIndex; i++)
                result += model.FiberArray[i].UserInputedLength > 0 ? 0 : model.DistancesMm[i];
            return result;
        }

        private double GetNotUserInputDistancesFromRefMm(OtdrDataKnownBlocks sorData, TraceModelForBaseRef model,
            int leftEquipmentIndex, int rightEquipmentIndex)
        {
            var result =
                sorData.GetDistanceBetweenLandmarksInMm(leftEquipmentIndex, rightEquipmentIndex);

            for (int i = leftEquipmentIndex; i < rightEquipmentIndex; i++)
            {
                if (model.FiberArray[i].UserInputedLength > 0)
                    result -= model.FiberArray[i].UserInputedLength * 1000 * LinK;
            }

            if (model.EquipArray[leftEquipmentIndex].CableReserveRight > 0)
                result -= model.EquipArray[leftEquipmentIndex].CableReserveRight * 1000 * LinK;
            for (int i = leftEquipmentIndex + 1; i < rightEquipmentIndex; i++)
            {
                if (model.EquipArray[i].CableReserveLeft > 0)
                    result -= model.EquipArray[i].CableReserveLeft * 1000 * LinK;
                if (model.EquipArray[i].CableReserveRight > 0)
                    result -= model.EquipArray[i].CableReserveRight * 1000 * LinK;
            }
            if (model.EquipArray[rightEquipmentIndex].CableReserveLeft > 0)
                result -= model.EquipArray[rightEquipmentIndex].CableReserveLeft * 1000 * LinK;

            return result;
        }
    }
}