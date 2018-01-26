using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.StringResources;
using Optixsoft.SorExaminer.OtdrDataFormat;

namespace Iit.Fibertest.Client
{
    public class BaseRefLandmarksCounter
    {
        private readonly ReadModel _readModel;
        private readonly IWindowManager _windowManager;
        private readonly BaseRefAdjuster _baseRefAdjuster;

        public BaseRefLandmarksCounter(ReadModel readModel, IWindowManager windowManager, 
            BaseRefAdjuster baseRefAdjuster)
        {
            _readModel = readModel;
            _windowManager = windowManager;
            _baseRefAdjuster = baseRefAdjuster;
        }

        // not only checks but adjust empty nodes if needed
        public bool IsBaseRefLandmarksCountMatched(OtdrDataKnownBlocks otdrKnownBlocks, Trace trace, BaseRefDto baseRefDto)
        {
            var landmarkCount = otdrKnownBlocks.LinkParameters.LandmarksCount;

            var equipments = _readModel.GetTraceEquipments(trace).ToList(); // without RTU
            var nodesCount = equipments.Count(eq => eq.Type > EquipmentType.AdjustmentPoint) + 1; // without adjustment points
            var equipmentsCount = equipments.Count(eq => eq.Type > EquipmentType.CableReserve) + 1; // sic! in this case CableReserve is not an equipment

            if (landmarkCount == nodesCount)
                return true;
            if (landmarkCount == equipmentsCount)
            {
                // add landmarks into OtdrDataKnownBlocks.LinkParameters for empty nodes
                _baseRefAdjuster.AddLandmarksForEmptyNodes(otdrKnownBlocks, trace);
                baseRefDto.SorBytes = otdrKnownBlocks.ToBytes();
                return true;
            }

            string errorStrings = BuildErrorStrings(landmarkCount, nodesCount, equipmentsCount);
            ShowError(errorStrings, trace, baseRefDto);
            return false;
        }

        private string BuildErrorStrings(int keyEventsCount, int nodesCount, int equipmentsCount)
        {
            return  string.Format(Resources.SID_Landmarks_count_in_reflectogram_is__0_, keyEventsCount) + Environment.NewLine +
                Environment.NewLine +
                string.Format(Resources.SID_Trace_s_node_count_is__0_, nodesCount) + Environment.NewLine +
                string.Format(Resources.SID_Trace_s_equipment_count_is__0_, equipmentsCount);
        }

        private void ShowError(string errorStrings, Trace trace, BaseRefDto baseRefDto)
        {
            var messageStrings = new List<string>() {
                string.Format(Resources.SID__0__base_is_not_compatible_with_trace, baseRefDto.BaseRefType.GetLocalizedFemaleString()),
                trace.Title, "", ""
            };
            messageStrings.Add(errorStrings);
            var vm = new MyMessageBoxViewModel(MessageType.Error, messageStrings, 4);
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }
    }
}