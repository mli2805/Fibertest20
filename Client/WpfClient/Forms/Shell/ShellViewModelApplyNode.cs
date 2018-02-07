using System;
using System.Linq;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Algorithms;
using Iit.Fibertest.Graph.Requests;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public partial class ShellViewModel
    {
        #region AddNodeIntoFiber
        /// <summary>
        /// Attention! Mind the difference with Fibertest 1.5
        /// This command for add node (well) only!
        /// Equipment should be added by separate command!
        /// </summary>
        private AddNodeIntoFiber PrepareCommand(RequestAddNodeIntoFiber request)
        {
            if (request.InjectionType != EquipmentType.AdjustmentPoint  && IsFiberContainedInAnyTraceWithBase(request.FiberId))
            {
                _windowManager.ShowDialogWithAssignedOwner(new MyMessageBoxViewModel(MessageType.Error, Resources.SID_It_s_impossible_to_change_trace_with_base_reflectogram));
                return null;
            }

            return new AddNodeIntoFiber()
            {
                Id = Guid.NewGuid(),
                EquipmentId = Guid.NewGuid(),
                Position = request.Position,
                InjectionType = request.InjectionType,
                FiberId = request.FiberId,
                NewFiberId1 = Guid.NewGuid(),
                NewFiberId2 = Guid.NewGuid()
            };
        }

        private bool IsFiberContainedInAnyTraceWithBase(Guid fiberId)
        {
            var fiber = ReadModel.Fibers.First(f => f.Id == fiberId);
            return ReadModel.Traces.Where(t => t.HasAnyBaseRef).ToList().Any(trace => Topo.GetFiberIndexInTrace(trace, fiber) != -1);
        }
        #endregion
    }
}
