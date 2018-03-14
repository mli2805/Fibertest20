﻿using System;
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

        public void ShowMonitoringResult(MeasurementAdded evnt)
        {
            _model.ChangeTraceColor(evnt.TraceId, evnt.TraceState);

            _model.CleanAccidentPlacesOnTrace(evnt.TraceId); // accidents on trace could change, so old should be cleaned and new drawn
            if (evnt.TraceState != FiberState.Ok && evnt.TraceState != FiberState.NoFiber)
                evnt.Accidents.ForEach(a => ShowAccidentPlaceOnTrace(a, evnt.TraceId));
        }

        private void ShowAccidentPlaceOnTrace(AccidentOnTrace accident, Guid traceId)
        {
            switch (accident)
            {
                case AccidentAsNewEvent accidentAsNewEvent:
                    ShowBetweenNodes(accidentAsNewEvent, traceId);
                    return;
                case AccidentInOldEvent accidentInOldEvent:
                    if (accidentInOldEvent.OpticalTypeOfAccident == OpticalAccidentType.LossCoeff)
                        ShowBadSegment(accidentInOldEvent, traceId);
                    else
                        ShowInNode(accidentInOldEvent, traceId);
                    return;
                default: return;
            }
        }

        private void ShowBetweenNodes(AccidentAsNewEvent accidentAsNewEvent, Guid traceId)
        {
            var accidentGps = _accidentPlaceLocator.GetAccidentGps(accidentAsNewEvent);
            if (accidentGps == null) return;

           AddAccidentNode((PointLatLng)accidentGps, traceId);
        }

        private void ShowInNode(AccidentInOldEvent accidentInOldEvent, Guid traceId)
        {
            var nodeVm = _model.GetNodeByLandmarkIndex(traceId, accidentInOldEvent.BrokenLandmarkIndex);

            if (nodeVm == null) return;

            AddAccidentNode(nodeVm.Position, traceId);
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

        private void ShowBadSegment(AccidentInOldEvent accidentInOldEvent, Guid traceId)
        {
//            var leftNodeId = traceVm.Nodes[accidentInOldEvent.BrokenLandmarkIndex - 1];
//            var rightNodeId = traceVm.Nodes[accidentInOldEvent.BrokenLandmarkIndex];
//            var fiberVm = _model.GetFiberByNodes(leftNodeId, rightNodeId);

            var fiberVm = _model.GetFiberByLandmarkIndexes(traceId, accidentInOldEvent.BrokenLandmarkIndex - 1,
                accidentInOldEvent.BrokenLandmarkIndex);


            fiberVm.AddBadSegment(traceId);
        }

    }
}