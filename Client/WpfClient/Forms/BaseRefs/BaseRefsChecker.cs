﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WpfCommonViews;

using Optixsoft.SorExaminer.OtdrDataFormat;

namespace Iit.Fibertest.Client
{
    public class BaseRefsChecker
    {
        private readonly Model _readModel;
        private readonly IWindowManager _windowManager;
        private readonly CurrentGis _currentGis;
        private readonly BaseRefMeasParamsChecker _baseRefMeasParamsChecker;
        private readonly BaseRefLandmarksChecker _baseRefLandmarksChecker;
        private readonly GraphGpsCalculator _graphGpsCalculator;
        private readonly BaseRefLandmarksTool _baseRefLandmarksTool;

        public BaseRefsChecker(Model readModel, IWindowManager windowManager, CurrentGis currentGis,
            BaseRefMeasParamsChecker baseRefMeasParamsChecker, BaseRefLandmarksChecker baseRefLandmarksChecker,
            GraphGpsCalculator graphGpsCalculator,  
            BaseRefLandmarksTool baseRefLandmarksTool)
        {
            _readModel = readModel;
            _windowManager = windowManager;
            _currentGis = currentGis;
            _baseRefMeasParamsChecker = baseRefMeasParamsChecker;
            _baseRefLandmarksChecker = baseRefLandmarksChecker;
            _graphGpsCalculator = graphGpsCalculator;
            _baseRefLandmarksTool = baseRefLandmarksTool;
        }

        public bool IsBaseRefsAcceptable(List<BaseRefDto> baseRefsDto, Trace trace)
        {
            try
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

                    var comparisonResult = _baseRefLandmarksChecker.IsBaseRefLandmarksCountMatched(
                        otdrKnownBlocks, trace, baseRefDto.BaseRefType.GetLocalizedFemaleString());
                    if (comparisonResult == CountMatch.Error)
                        return false;
 
                    if (!AreFirstAndLastLandmarksAssociatedWithKeyEvents(otdrKnownBlocks, baseRefHeader))
                        return false;

                    if (_currentGis.IsGisOn && baseRefDto.BaseRefType == BaseRefType.Precise)
                        if (!IsDistanceLengthAcceptable(otdrKnownBlocks, trace))
                            return false;

                    baseRefDto.SorBytes = _baseRefLandmarksTool.ApplyTraceToBaseRef(otdrKnownBlocks, trace,
                        comparisonResult == CountMatch.LandmarksMatchEquipments);
                }
                return true;
            }
            catch (Exception e)
            {
                var mess = Resources.SID_Error_while_base_ref_acceptability_checking_;
                var strs = new List<string>() { mess, "", e.Message };
                var vm = new MyMessageBoxViewModel(MessageType.Error, strs, 2);
                _windowManager.ShowDialogWithAssignedOwner(vm);
                return false;
            }
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

        private bool AreFirstAndLastLandmarksAssociatedWithKeyEvents(OtdrDataKnownBlocks otdrDataKnownBlocks, string errorHeader)
        {
            var landmarks = otdrDataKnownBlocks.LinkParameters.LandmarkBlocks;
            if (landmarks[0].RelatedEventNumber == 0 || landmarks[landmarks.Length - 1].RelatedEventNumber == 0)
            {
                var message = Resources.SID_First_and_last_landmarks_should_be_associated_with_key_events_;
                var vm = new MyMessageBoxViewModel(MessageType.Error, new List<string>() { errorHeader, "", message }, 2);
                _windowManager.ShowDialogWithAssignedOwner(vm);

                return false;
            }

            return true;
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

        private bool IsDistanceLengthAcceptable(OtdrDataKnownBlocks otdrKnownBlocks, Trace trace)
        {
            var gpsDistance = $@"{_graphGpsCalculator.CalculateTraceGpsLengthKm(trace):#,0.##}";
            var opticalLength = $@"{otdrKnownBlocks.GetTraceLengthKm():#,0.##}";
            var message = string.Format(Resources.SID_Trace_length_on_map_is__0__km, gpsDistance) +
                   Environment.NewLine + string.Format(Resources.SID_Optical_length_is__0__km, opticalLength);
            var vmc = new MyMessageBoxViewModel(MessageType.Confirmation,
                AssembleConfirmation(message));
            _windowManager.ShowDialogWithAssignedOwner(vmc);
            return vmc.IsAnswerPositive;
        }

        private List<MyMessageBoxLineModel> AssembleConfirmation(string message)
        {
            var result = new List<MyMessageBoxLineModel>();
            result.Add(new MyMessageBoxLineModel() { Line = BaseRefType.Precise.GetLocalizedFemaleString() + Resources.SID__base_ });
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
