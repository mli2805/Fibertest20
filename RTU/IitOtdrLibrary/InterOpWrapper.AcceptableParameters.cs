using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.IitOtdrLibrary
{
    public partial class InterOpWrapper
    {
        public TreeOfAcceptableMeasParams GetTreeOfAcceptableMeasParams()
        {
            var result = new TreeOfAcceptableMeasParams();
            var units = GetArrayOfVariantsForParam(ServiceFunctionFirstParam.Unit);
            for (int i = 0; i < units.Length; i++)
            {
                SetParam(ServiceFunctionFirstParam.Unit, i);
                result.Units.Add(units[i], GetBranchOfAcceptableMeasParams());
            }
            return result;
        }

        private BranchOfAcceptableMeasParams GetBranchOfAcceptableMeasParams()
        {
            var result = new BranchOfAcceptableMeasParams();

            result.BackscatteredCoefficient = double.Parse(GetArrayOfVariantsForParam(ServiceFunctionFirstParam.Bc)[0], new CultureInfo("en-US"));
            result.RefractiveIndex = double.Parse(GetArrayOfVariantsForParam(ServiceFunctionFirstParam.Ri)[0], new CultureInfo("en-US"));

            var distances = GetArrayOfVariantsForParam(ServiceFunctionFirstParam.Lmax);
            for (int i = 0; i < distances.Length; i++)
            {
                SetParam(ServiceFunctionFirstParam.Lmax, i);
                result.Distances.Add(distances[i], GetLeafOfAcceptableMeasParams());
            }
            return result;
        }

        private LeafOfAcceptableMeasParams GetLeafOfAcceptableMeasParams()
        {
            var result = new LeafOfAcceptableMeasParams();
            result.Resolutions = GetArrayOfVariantsForParam(ServiceFunctionFirstParam.Res);
            result.PulseDurations = GetArrayOfVariantsForParam(ServiceFunctionFirstParam.Pulse);

            SetParam(ServiceFunctionFirstParam.IsTime, 1);
            result.PeriodsToAverage = GetArrayOfVariantsForParam(ServiceFunctionFirstParam.Time);

            SetParam(ServiceFunctionFirstParam.IsTime, 0);
            result.MeasCountsToAverage = GetArrayOfVariantsForParam(ServiceFunctionFirstParam.Navr);
            return result;
        }

        // Measurement Client
        public void SetMeasParamsByPosition(List<MeasParamByPosition> list)
        {
            foreach (var measParam in list)
            {
                _rtuLogger.AppendLine($"{measParam.Param} - {measParam.Position}", 0, 3);
                SetParam(measParam.Param, measParam.Position);
                Thread.Sleep(200);
            }
        }

        public List<MeasParamByPosition> ValuesToPositions(List<MeasParamByPosition> allParams, VeexMeasOtdrParameters measParams,
            TreeOfAcceptableMeasParams treeOfAcceptableMeasParams)
        {
            var result = new List<MeasParamByPosition>(){
                allParams.First(p=>p.Param == ServiceFunctionFirstParam.Unit),
                allParams.First(p=>p.Param == ServiceFunctionFirstParam.Bc),
                allParams.First(p=>p.Param == ServiceFunctionFirstParam.Ri),
                new MeasParamByPosition(){Param = ServiceFunctionFirstParam.IsTime, Position = 1 },
            };

            var unit = treeOfAcceptableMeasParams.Units.Values.ToArray()[0];

            var lmaxStr = measParams.distanceRange;
            result.Add(new MeasParamByPosition() { Param = ServiceFunctionFirstParam.Lmax, Position = unit.Distances.Keys.ToList().IndexOf(lmaxStr) });
            var leaf = unit.Distances[lmaxStr];

            result.Add(new MeasParamByPosition
                { Param = ServiceFunctionFirstParam.Res, Position = Array.IndexOf(leaf.Resolutions, measParams.resolution) });
            result.Add(new MeasParamByPosition
                { Param = ServiceFunctionFirstParam.Pulse, Position = Array.IndexOf(leaf.PulseDurations, measParams.pulseDuration) });
            result.Add(new MeasParamByPosition
                { Param = ServiceFunctionFirstParam.Time, Position = Array.IndexOf(leaf.PeriodsToAverage, measParams.averagingTime) });

            return result;
        }
    }
}
