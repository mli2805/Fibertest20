using System;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Algorithms;
using Iit.Fibertest.Graph.Requests;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class GrmNodeRequests
    {
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly IWindowManager _windowManager;
        private readonly ReadModel _readModel;

        public GrmNodeRequests(IWcfServiceForClient c2DWcfManager, IWindowManager windowManager, ReadModel readModel)
        {
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;
            _readModel = readModel;
        }

        public async Task AddNodeIntoFiber(RequestAddNodeIntoFiber request)
        {
            var cmd = PrepareAddNodeIntoFiber(request);
            if (cmd == null)
                return;
            var message = await _c2DWcfManager.SendCommandAsObj(cmd);
            if (message != null)
                _windowManager.ShowDialogWithAssignedOwner(new MyMessageBoxViewModel(MessageType.Error, message));
        }

        private AddNodeIntoFiber PrepareAddNodeIntoFiber(RequestAddNodeIntoFiber request)
        {
            if (request.InjectionType != EquipmentType.AdjustmentPoint && IsFiberContainedInAnyTraceWithBase(request.FiberId))
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
            var fiber = _readModel.Fibers.First(f => f.Id == fiberId);
            return _readModel.Traces.Where(t => t.HasAnyBaseRef).ToList().Any(trace => Topo.GetFiberIndexInTrace(trace, fiber) != -1);
        }
    }
}