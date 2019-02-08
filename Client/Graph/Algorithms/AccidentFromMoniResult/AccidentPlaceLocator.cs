using System;
using System.Collections.Generic;
using System.Linq;
using GMap.NET;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Graph
{
   public class AccidentPlaceLocator
   {
      private readonly IMyLog _logFile;
      private readonly Model _model;

      public AccidentPlaceLocator(IMyLog logFile, Model model)
      {
         _logFile = logFile;
         _model = model;
      }

      public void SetAccidentInNewEventGps(AccidentOnTraceV2 accident, Guid traceId)
      {
         var trace = _model.Traces.First(t => t.TraceId == traceId);

         var distances = GetGpsDistancesOfSegmentsBetweenLandmarks(accident, trace, out Node leftNodeVm, out Node rightNodeVm);
         GetCableReserves(accident, traceId, out double leftReserveM, out double rightReserveM);
         var distanceBetweenTwoNodesOnGraphM = distances.Sum();

         var opticalLengthM = (accident.Right.ToRtuOpticalDistanceKm - accident.Left.ToRtuOpticalDistanceKm) * 1000;
         var coeff = opticalLengthM / (distanceBetweenTwoNodesOnGraphM + leftReserveM + rightReserveM);

         var distanceToAccidentOnGraphM = (accident.AccidentToRtuOpticalDistanceKm - accident.Left.ToRtuOpticalDistanceKm) * 1000 / coeff;

         if (distanceToAccidentOnGraphM <= leftReserveM)
         {
            accident.AccidentCoors = leftNodeVm.Position;
            return;
         }

         if (distanceToAccidentOnGraphM > leftReserveM + distanceBetweenTwoNodesOnGraphM)
         {
            accident.AccidentCoors = rightNodeVm.Position;
            return;
         }

         var segmentIndex = -1;
         var distancesSum = leftReserveM;
         while (distancesSum < distanceToAccidentOnGraphM)
         {
            segmentIndex++;
            distancesSum = distancesSum + distances[segmentIndex];
         }

         accident.AccidentCoors = GetPointOnBrokenSegment(trace,
             (distances[segmentIndex] - (distancesSum - distanceToAccidentOnGraphM)) / distances[segmentIndex],
             trace.NodeIds.IndexOf(leftNodeVm.NodeId) + segmentIndex);
      }


      // y = (x * (y2 - y1) + (x2 * y1 - x1 * y2)) / (x2 - x1);
      private PointLatLng GetPointOnBrokenSegment(Trace trace, double procentOfSegmentUptoAccident, int leftNodeIndex)
      {
         var leftNode = _model.Nodes.First(n => n.NodeId == trace.NodeIds[leftNodeIndex]);
         var rightNode = _model.Nodes.First(n => n.NodeId == trace.NodeIds[leftNodeIndex + 1]);
         var lat1 = leftNode.Position.Lat;
         var lng1 = leftNode.Position.Lng;
         var lat2 = rightNode.Position.Lat;
         var lng2 = rightNode.Position.Lng;

         var lat = lat1 + (lat2 - lat1) * procentOfSegmentUptoAccident;
         var lng = lng1 + (lng2 - lng1) * procentOfSegmentUptoAccident;

         return new PointLatLng(lat, lng);
      }

      private void GetCableReserves(AccidentOnTraceV2 accident, Guid traceId, out double leftReserveM, out double rightReserveM)
      {
         var equipmentsWithoutPointsAndRtu = _model.GetTraceEquipmentsExcludingAdjustmentPoints(traceId).ToList();
         leftReserveM = GetCableReserve(equipmentsWithoutPointsAndRtu, accident.Left.LandmarkIndex, true);
         rightReserveM = GetCableReserve(equipmentsWithoutPointsAndRtu, accident.Right.LandmarkIndex, false);
      }

      private double GetCableReserve(List<Equipment> equipmentsWithoutPointsAndRtu, int landmarkIndex, bool isLeftLandmark)
      {
         if (landmarkIndex == 0)
            return 0; // RTU cannot contain cable reserve
         var equipment = equipmentsWithoutPointsAndRtu[landmarkIndex - 1];
         if (equipment.Type == EquipmentType.CableReserve) return (double)equipment.CableReserveLeft / 2;
         return isLeftLandmark ? equipment.CableReserveRight : equipment.CableReserveLeft;
      }

      private List<double> GetGpsDistancesOfSegmentsBetweenLandmarks(AccidentOnTraceV2 accident, Trace trace, out Node leftNode, out Node rightNode)
      {
         var withoutPoints = _model.GetTraceNodesExcludingAdjustmentPoints(trace.TraceId).ToList();
         leftNode = _model.Nodes.FirstOrDefault(n => n.NodeId == withoutPoints[accident.Left.LandmarkIndex]);
         rightNode = _model.Nodes.FirstOrDefault(n => n.NodeId == withoutPoints[accident.Right.LandmarkIndex]);

         if (leftNode == null)
         {
            _logFile.AppendLine($@"Node {withoutPoints[accident.Left.LandmarkIndex].First6()} not found");
            return null;
         }
         if (rightNode == null)
         {
            _logFile.AppendLine($@"Node {withoutPoints[accident.Right.LandmarkIndex].First6()} not found");
            return null;
         }

         var indexOfLeft = trace.NodeIds.IndexOf(leftNode.NodeId);
         var indexOfRight = trace.NodeIds.IndexOf(rightNode.NodeId);
         var result = new List<double>();
         var fromNode = leftNode;
         for (int i = indexOfLeft + 1; i < indexOfRight; i++)
         {
            var toNode = _model.Nodes.First(n => n.NodeId == trace.NodeIds[i]);
            result.Add(GpsCalculator.GetDistanceBetweenPointLatLng(fromNode.Position, toNode.Position));
            fromNode = toNode;
         }
         result.Add(GpsCalculator.GetDistanceBetweenPointLatLng(fromNode.Position, rightNode.Position));
         return result;
      }
   }
}