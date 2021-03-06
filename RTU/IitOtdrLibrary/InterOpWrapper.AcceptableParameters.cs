﻿using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.IitOtdrLibrary
{
    public partial class InterOpWrapper
    {
        public TreeOfAcceptableMeasParams GetTreeOfAcceptableMeasParams()
        {
            var result = new TreeOfAcceptableMeasParams();
            var units = ParseLineOfVariantsForParam(ServiceFunctionFirstParam.Unit);
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

            result.BackscatteredCoefficient = double.Parse(ParseLineOfVariantsForParam(ServiceFunctionFirstParam.Bc)[0], new CultureInfo("en-US"));
            result.RefractiveIndex = double.Parse(ParseLineOfVariantsForParam(ServiceFunctionFirstParam.Ri)[0], new CultureInfo("en-US"));

            var distances = ParseLineOfVariantsForParam(ServiceFunctionFirstParam.Lmax);
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
            result.Resolutions = ParseLineOfVariantsForParam(ServiceFunctionFirstParam.Res);
            result.PulseDurations = ParseLineOfVariantsForParam(ServiceFunctionFirstParam.Pulse);

            SetParam(ServiceFunctionFirstParam.IsTime, 1);
            result.PeriodsToAverage = ParseLineOfVariantsForParam(ServiceFunctionFirstParam.Time);

            SetParam(ServiceFunctionFirstParam.IsTime, 0);
            result.MeasCountsToAverage = ParseLineOfVariantsForParam(ServiceFunctionFirstParam.Navr);
            return result;
        }
        
        public void SetMeasurementParametersFromUserInput(List<MeasParam> list)
        {
            foreach (var measParam in list)
            {
                var prms = ParseLineOfVariantsForParam(measParam.Param);
                Thread.Sleep(50);

                if ((int)measParam.Param > 8)
                    _rtuLogger.AppendLine($"{measParam.Param.ToString()} - {measParam.Value}", 0, 3);
                else _rtuLogger.AppendLine($"{measParam.Param.ToString()} - {measParam.Value} ({prms[measParam.Value]})", 0, 3);
                Thread.Sleep(50);
                SetParam(measParam.Param, measParam.Value);
            }
        }
    }
}
