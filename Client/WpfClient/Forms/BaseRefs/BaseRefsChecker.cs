using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.StringResources;
using Optixsoft.SorExaminer.OtdrDataFormat;

namespace Iit.Fibertest.Client
{
    public class BaseRefsChecker
    {
        private readonly ReadModel _readModel;
        private readonly IWindowManager _windowManager;
        private readonly GraphGpsCalculator _graphGpsCalculator;

        public BaseRefsChecker(ReadModel readModel, IWindowManager windowManager, GraphGpsCalculator graphGpsCalculator)
        {
            _readModel = readModel;
            _windowManager = windowManager;
            _graphGpsCalculator = graphGpsCalculator;
        }

        public bool IsBaseRefsAcceptable(List<BaseRefDto> baseRefsDto, Trace trace)
        {
            var rtu = _readModel.Rtus.FirstOrDefault(r => r.Id == trace.RtuId);
            if (rtu == null)
                return false;

            foreach (var baseRefDto in baseRefsDto)
            {
                var baseRefHeader = baseRefDto.BaseRefType.GetLocalizedFemaleString() + Resources.SID__base_;

                var message = SorData.TryGetFromBytes(baseRefDto.SorBytes, out var otdrKnownBlocks);
                if (message != "")
                {
                    var vm = new MyMessageBoxViewModel(MessageType.Error, new List<string>(){baseRefHeader, "", "", message});
                    _windowManager.ShowDialogWithAssignedOwner(vm);
                    return false;
                }

                message = BaseRefMeasParamsChecker.IsBaseRefHasAcceptableMeasParams(otdrKnownBlocks, rtu.AcceptableMeasParams);
                if (message != "")
                {
                    var vm = new MyMessageBoxViewModel(MessageType.Error, new List<string>() {
                        baseRefHeader, "", "", Resources.SID_Measurement_parameters_are_not_compatible_with_this_RTU, "", message
                    }, 5);
                    _windowManager.ShowDialogWithAssignedOwner(vm);
                    return false;
                }

                message = IsBaseRefCompatibleWithTrace(otdrKnownBlocks, trace);
                if (message != "")
                {
                    var vm = new MyMessageBoxViewModel(MessageType.Error, new List<string>() {
                        string.Format(Resources.SID__0__base_is_not_compatible_with_trace, baseRefDto.BaseRefType.GetLocalizedFemaleString()),
                        trace.Title, "", "", message
                    }, 4);
                    _windowManager.ShowDialogWithAssignedOwner(vm);
                    return false;
                }

                message = CompareDistances(otdrKnownBlocks, trace);
                var vmc = new MyMessageBoxViewModel(MessageType.Confirmation, AssembleConfirmation(baseRefDto, message));
                _windowManager.ShowDialogWithAssignedOwner(vmc);
                if (!vmc.IsAnswerPositive)
                    return false;
            }
            return true;
        }

        private List<MyMessageBoxLineModel> AssembleConfirmation(BaseRefDto baseRefDto, string message)
        {
            var result = new List<MyMessageBoxLineModel>();
            result.Add(new MyMessageBoxLineModel(){ Line = baseRefDto.BaseRefType.GetLocalizedFemaleString() + Resources.SID__base_});
            result.Add(new MyMessageBoxLineModel(){ Line = "" });
            result.Add(new MyMessageBoxLineModel(){ Line = "" });
            result.Add(new MyMessageBoxLineModel(){ Line = message, FontWeight = FontWeights.Bold });
            result.Add(new MyMessageBoxLineModel(){ Line = "" });
            result.Add(new MyMessageBoxLineModel(){ Line = "" });
            result.Add(new MyMessageBoxLineModel(){ Line = Resources.SID_Assign_base_reflectogram_ });
            return result;
        }

        private string IsBaseRefCompatibleWithTrace(OtdrDataKnownBlocks otdrKnownBlocks, Trace trace)
        {
            var keyEventsCount = otdrKnownBlocks.KeyEvents.KeyEvents.Length;
            var equipmentsCount = trace.Equipments.Count;
            if (keyEventsCount !=  equipmentsCount)
                return $@"{keyEventsCount} doesn't match {equipmentsCount}";
            return "";
        }

        private string CompareDistances(OtdrDataKnownBlocks otdrKnownBlocks, Trace trace)
        {
            var gpsDistance = $@"{_graphGpsCalculator.CalculateTraceGpsLength(trace):#,0.##}";
            var opticalLength = $@"{otdrKnownBlocks.GetTraceLengthKm():#,0.##}";
            return string.Format(Resources.SID_Trace_length_on_map_is__0__km, gpsDistance) + 
                   Environment.NewLine + string.Format(Resources.SID_Optical_length_is__0__km, opticalLength);
        }

     }
}
