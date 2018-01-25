using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Optixsoft.SorExaminer.OtdrDataFormat;

namespace Iit.Fibertest.Client
{
    public class BaseRefLandmarksCounter
    {
        private readonly ReadModel _readModel;
        private readonly IWindowManager _windowManager;

        public BaseRefLandmarksCounter(ReadModel readModel, IWindowManager windowManager)
        {
            _readModel = readModel;
            _windowManager = windowManager;
        }

        // not only checks but solve if possible
        public bool IsBaseRefLandmarksCountMatched(OtdrDataKnownBlocks otdrKnownBlocks, Trace trace, BaseRefDto baseRefDto)
        {
            var keyEventsCount = otdrKnownBlocks.KeyEvents.KeyEvents.Length;

            var equipments = _readModel.GetTraceEquipments(trace).ToList(); // without RTU
            var nodesCount = equipments.Count(eq => eq.Type > EquipmentType.AdjustmentPoint) + 1; // without adjustment points
            var equipmentsCount = equipments.Count(eq => eq.Type > EquipmentType.CableReserve) + 1; // sic! in this case CableReserve is not an equipment

            if (keyEventsCount == nodesCount)
                return true;
            if (keyEventsCount == equipmentsCount)
            {
                // add landmarks(keyEvents) into OtdrDataKnownBlocks on empty nodes places
                AddKeyEventsForEmptyNodes();
                return true;
            }

            string errorStrings = BuildErrorStrings(keyEventsCount, nodesCount, equipmentsCount);
            ShowError(errorStrings, trace, baseRefDto);
            return false;
        }

        private void AddKeyEventsForEmptyNodes() { }

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