﻿using System;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Optixsoft.SorExaminer.OtdrDataFormat;

namespace Iit.Fibertest.Graph
{
    public class BaseRefLandmarksTool
    {
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
            baseRefDto.SorBytes = ApplyTraceToBaseRef(otdrKnownBlocks, trace, otdrKnownBlocks.LinkParameters.LandmarkBlocks.Length < trace.NodeIds.Count);
        }

        public byte[] ApplyTraceToBaseRef(OtdrDataKnownBlocks otdrKnownBlocks, Trace trace,
            bool needToInsertLandmarksForEmptyNodes)
        {
            var traceModel = _readModel.GetTraceComponentsByIds(trace);
            var modelWithoutAdjustmentPoint = _traceModelBuilder.GetTraceModelWithoutAdjustmentPoints(traceModel);
            if (needToInsertLandmarksForEmptyNodes)
                InsertLandmarks(otdrKnownBlocks, modelWithoutAdjustmentPoint);

            SetLandmarksLocation(otdrKnownBlocks, modelWithoutAdjustmentPoint);

            AddNamesAndTypesForLandmarks(otdrKnownBlocks, modelWithoutAdjustmentPoint);

            return otdrKnownBlocks.ToBytes();
        }

        public void SetLandmarksLocation(OtdrDataKnownBlocks sorData, TraceModelForBaseRef model)
        {
            var landmarks = sorData.LinkParameters.LandmarkBlocks;

            for (int i = 1; i < model.EquipArray.Length; i++)
            {
                if (landmarks[i].RelatedEventNumber != 0) continue; // landmark is associated with keyEvent and we can't move it

                var ratio = GetRatioBaseRefToGraphAroundEmptyNode(sorData, model, i);
                landmarks[i].Location = landmarks[i - 1].Location + sorData.GetOwtFromMm((int)(model.DistancesMm[i - 1] * ratio));
            }
        }

        public void InsertLandmarks(OtdrDataKnownBlocks sorData, TraceModelForBaseRef model)
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
            // does not work for Reflect
            //sorData.GeneralParameters.Language = LanguageCode.Russian;

            var landmarks = sorData.LinkParameters.LandmarkBlocks;

            for (int i = 0; i < landmarks.Length; i++)
            {
                var landmarkTitle = model.NodeArray[i].Title;
                if (i != 0 && !string.IsNullOrEmpty(model.EquipArray[i].Title))
                    landmarkTitle = landmarkTitle + $@" / {model.EquipArray[i].Title}";

                landmarks[i].Comment = landmarkTitle; // utf8, TODO reflect.exe should understand this

                // does not work
                //                byte[] in_cp1251 = Encoding.GetEncoding(1251).GetBytes(landmarkTitle);
                //                landmarks[i].Comment = Encoding.GetEncoding(1251).GetString(in_cp1251);


                landmarks[i].Code = model.EquipArray[i].Type.ToLandmarkCode();
                landmarks[i].GpsLatitude = GisLabCalculator.GpsInSorFormat(model.NodeArray[i].Position.Lat);
                landmarks[i].GpsLongitude = GisLabCalculator.GpsInSorFormat(model.NodeArray[i].Position.Lng);
            }
        }
        private double GetRatioBaseRefToGraphAroundEmptyNode(OtdrDataKnownBlocks sorData, TraceModelForBaseRef model, int emptyNodeIndex)
        {
            var leftRealEquipmentIndex = emptyNodeIndex - 1;
            while (model.EquipArray[leftRealEquipmentIndex].Type <= EquipmentType.CableReserve) leftRealEquipmentIndex--;
            var rightRealEquipmentIndex = emptyNodeIndex + 1;
            while (model.EquipArray[rightRealEquipmentIndex].Type <= EquipmentType.CableReserve) rightRealEquipmentIndex++;

            var onGraph = GetDistanceBetweenRealEquipmentsOnGraphMm(model, leftRealEquipmentIndex, rightRealEquipmentIndex);
            var onBaseRef =
                sorData.GetDistanceBetweenLandmarksInMm(leftRealEquipmentIndex, rightRealEquipmentIndex);
            return ((double)onBaseRef) / onGraph;
        }

        private int GetDistanceBetweenRealEquipmentsOnGraphMm(TraceModelForBaseRef model, int leftEquipmentIndex, int rightEquipmentIndex)
        {
            if (rightEquipmentIndex - leftEquipmentIndex == 1)
                return model.DistancesMm[leftEquipmentIndex];

            return model.DistancesMm[leftEquipmentIndex] +
                   GetDistanceBetweenRealEquipmentsOnGraphMm(model, leftEquipmentIndex + 1, rightEquipmentIndex);
        }
    }
}