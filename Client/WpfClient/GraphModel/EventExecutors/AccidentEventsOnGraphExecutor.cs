using System;
using System.Linq;
using GMap.NET;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class AccidentEventsOnGraphExecutor
    {
        private readonly GraphReadModel _model;
        private readonly AccidentPlaceLocator _accidentPlaceLocator;

        public AccidentEventsOnGraphExecutor(GraphReadModel model, AccidentPlaceLocator accidentPlaceLocator)
        {
            _model = model;
            _accidentPlaceLocator = accidentPlaceLocator;
        }

        public void ShowMonitoringResult(MonitoringResultShown evnt)
        {
            var traceVm = _model.Data.Traces.First(t => t.Id == evnt.TraceId);
            traceVm.State = evnt.TraceState;
            _model.ChangeTraceColor(evnt.TraceId, traceVm.Nodes, traceVm.State);

            _model.CleanAccidentPlacesOnTrace(traceVm); // accidents on trace could change, so old should be cleaned and new drawn
            if (traceVm.State != FiberState.Ok)
                evnt.Accidents.ForEach(a => ShowAccidentPlaceOnTrace(a, traceVm));
        }

        private void ShowAccidentPlaceOnTrace(AccidentOnTrace accident, TraceVm traceVm)
        {
            switch (accident)
            {
                case AccidentAsNewEvent accidentAsNewEvent:
                    ShowBetweenNodes(accidentAsNewEvent, traceVm);
                    return;
                case AccidentInOldEvent accidentInOldEvent:
                    if (accidentInOldEvent.OpticalTypeOfAccident == OpticalAccidentType.LossCoeff)
                        ShowBadSegment(accidentInOldEvent, traceVm);
                    else
                        ShowInNode(accidentInOldEvent, traceVm);
                    return;
                default: return;
            }
        }

        private void ShowBetweenNodes(AccidentAsNewEvent accidentAsNewEvent, TraceVm traceVm)
        {
            var accidentGps = _accidentPlaceLocator.GetAccidentGps(accidentAsNewEvent);
            if (accidentGps == null) return;

           AddAccidentNode((PointLatLng)accidentGps, traceVm.Id);
        }

        private void ShowInNode(AccidentInOldEvent accidentInOldEvent, TraceVm traceVm)
        {
            var nodeVm = _model.GetNodeByLandmarkIndex(traceVm, accidentInOldEvent.BrokenLandmarkIndex);

            if (nodeVm == null) return;

            AddAccidentNode(nodeVm.Position, traceVm.Id);
        }

        private void AddAccidentNode(PointLatLng accidentGps, Guid traceId)
        {
            var accidentNode = new NodeVm()
            {
                Id = Guid.NewGuid(),
                Position = accidentGps,
                Type = EquipmentType.AccidentPlace,
                AccidentOnTraceVmId = traceId,
            };
//            _model.Data.Equipments.Add(new EquipmentVm()
//            {
//                Id = Guid.NewGuid(),
//                Node = accidentNode,
//                Type = EquipmentType.AccidentPlace
//            });
            _model.Data.Nodes.Add(accidentNode);
        }

        private void ShowBadSegment(AccidentInOldEvent accidentInOldEvent, TraceVm traceVm)
        {
//            var leftNodeId = traceVm.Nodes[accidentInOldEvent.BrokenLandmarkIndex - 1];
//            var rightNodeId = traceVm.Nodes[accidentInOldEvent.BrokenLandmarkIndex];
//            var fiberVm = _model.GetFiberByNodes(leftNodeId, rightNodeId);

            var fiberVm = _model.GetFiberByLandmarkIndexes(traceVm, accidentInOldEvent.BrokenLandmarkIndex - 1,
                accidentInOldEvent.BrokenLandmarkIndex);


            fiberVm.AddBadSegment(traceVm.Id);
        }

    }
}