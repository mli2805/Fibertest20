﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Algorithms.ToolKit;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.StringResources;
using Optixsoft.SorExaminer.OtdrDataFormat;

namespace Iit.Fibertest.Client
{
    public class BaseRefsChecker
    {
        private readonly ReadModel _readModel;
        private readonly IWindowManager _windowManager;
        private readonly BaseRefMeasParamsChecker _baseRefMeasParamsChecker;
        private readonly BaseRefLandmarksChecker _baseRefLandmarksChecker;
        private readonly GraphGpsCalculator _graphGpsCalculator;
        private readonly BaseRefAdjuster _baseRefAdjuster;

        public BaseRefsChecker(ReadModel readModel, IWindowManager windowManager,
            BaseRefMeasParamsChecker baseRefMeasParamsChecker, BaseRefLandmarksChecker baseRefLandmarksChecker,
            GraphGpsCalculator graphGpsCalculator, BaseRefAdjuster baseRefAdjuster)
        {
            _readModel = readModel;
            _windowManager = windowManager;
            _baseRefMeasParamsChecker = baseRefMeasParamsChecker;
            _baseRefLandmarksChecker = baseRefLandmarksChecker;
            _graphGpsCalculator = graphGpsCalculator;
            _baseRefAdjuster = baseRefAdjuster;
        }

        public bool IsBaseRefsAcceptable(List<BaseRefDto> baseRefsDto, Trace trace)
        {
            var rtu = _readModel.Rtus.FirstOrDefault(r => r.Id == trace.RtuId);
            if (rtu == null)
                return false;

            foreach (var baseRefDto in baseRefsDto)
            {
                if (baseRefDto.Id == Guid.Empty) continue;
                var baseRefHeader = baseRefDto.BaseRefType.GetLocalizedFemaleString() + Resources.SID__base_;

                var otdrKnownBlocks = GetFromBytes(baseRefDto.SorBytes, baseRefHeader);
                if (otdrKnownBlocks == null) return false;

                if (!_baseRefMeasParamsChecker.IsBaseRefHasAcceptableMeasParams(otdrKnownBlocks, rtu.AcceptableMeasParams, baseRefHeader))
                    return false;

                if (!HasBaseThresholds(otdrKnownBlocks, baseRefHeader))
                    return false;

                if (!_baseRefLandmarksChecker.IsBaseRefLandmarksCountMatched(
                    otdrKnownBlocks, trace, baseRefDto.BaseRefType.GetLocalizedFemaleString()))
                        return false;
                _baseRefAdjuster.AddLandmarksForEmptyNodes(otdrKnownBlocks, trace);
                _baseRefAdjuster.AddNamesForLandmarks(otdrKnownBlocks, trace);
                baseRefDto.SorBytes = otdrKnownBlocks.ToBytes();

                if (baseRefDto.BaseRefType == BaseRefType.Precise)
                    if (!IsDistanceLengthAcceptable(otdrKnownBlocks, trace, baseRefDto))
                        return false;
            }
            return true;
        }

        private OtdrDataKnownBlocks GetFromBytes(byte[] sorBytes, string errorHeader)
        {
            var message = SorData.TryGetFromBytes(sorBytes, out var otdrKnownBlocks);
            if (message == "")
                return otdrKnownBlocks;
            var vm = new MyMessageBoxViewModel(MessageType.Error, new List<string>() { errorHeader, "", "", message });
            _windowManager.ShowDialogWithAssignedOwner(vm);
            return null;
        }

        private bool HasBaseThresholds(OtdrDataKnownBlocks otdrKnownBlocks, string errorHeader)
        {
            if (otdrKnownBlocks.RftsParameters.LevelsCount == 0)
            {
                var message = Resources.SID_There_are_no_thresholds_for_comparison;
                var vm = new MyMessageBoxViewModel(MessageType.Error, new List<string>() { errorHeader, "", message }, 2);
                _windowManager.ShowDialogWithAssignedOwner(vm);

                return false;
            }
            return true;
        }

        private bool IsDistanceLengthAcceptable(OtdrDataKnownBlocks otdrKnownBlocks, Trace trace, BaseRefDto baseRefDto)
        {
            var gpsDistance = $@"{_graphGpsCalculator.CalculateTraceGpsLengthKm(trace):#,0.##}";
            var opticalLength = $@"{otdrKnownBlocks.GetTraceLengthKm():#,0.##}";
            var message = string.Format(Resources.SID_Trace_length_on_map_is__0__km, gpsDistance) +
                   Environment.NewLine + string.Format(Resources.SID_Optical_length_is__0__km, opticalLength);
            var vmc = new MyMessageBoxViewModel(MessageType.Confirmation,
                AssembleConfirmation(baseRefDto, message));
            _windowManager.ShowDialogWithAssignedOwner(vmc);
            return vmc.IsAnswerPositive;
        }

        private List<MyMessageBoxLineModel> AssembleConfirmation(BaseRefDto baseRefDto, string message)
        {
            var result = new List<MyMessageBoxLineModel>();
            result.Add(new MyMessageBoxLineModel() { Line = baseRefDto.BaseRefType.GetLocalizedFemaleString() + Resources.SID__base_ });
            result.Add(new MyMessageBoxLineModel() { Line = "" });
            result.Add(new MyMessageBoxLineModel() { Line = "" });
            result.Add(new MyMessageBoxLineModel() { Line = message, FontWeight = FontWeights.Bold });
            result.Add(new MyMessageBoxLineModel() { Line = "" });
            result.Add(new MyMessageBoxLineModel() { Line = "" });
            result.Add(new MyMessageBoxLineModel() { Line = Resources.SID_Assign_base_reflectogram_ });
            return result;
        }
    }
}
