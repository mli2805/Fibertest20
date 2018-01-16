using Iit.Fibertest.Dto;

namespace Iit.Fibertest.IitOtdrLibrary
{
    public partial class InterOpWrapper
    {
        public TreeOfAcceptableMeasParams GetTreeOfAcceptableMeasParams()
        {
            var result = new TreeOfAcceptableMeasParams();
            var units = ParseLineOfVariantsForParam((int)ServiceFunctionFirstParam.Unit);
            for (int i = 0; i < units.Length; i++)
            {
                SetParam((int)ServiceFunctionFirstParam.Unit, i);
                result.Units.Add(units[i], GetBranchOfAcceptableMeasParams());
            }
            return result;
        }

        private BranchOfAcceptableMeasParams GetBranchOfAcceptableMeasParams()
        {
            var result = new BranchOfAcceptableMeasParams();
            var distances = ParseLineOfVariantsForParam((int)ServiceFunctionFirstParam.Lmax);
            for (int i = 0; i < distances.Length; i++)
            {
                SetParam((int)ServiceFunctionFirstParam.Lmax, i);
                result.Distances.Add(distances[i], GetLeafOfAcceptableMeasParams());
            }
            return result;
        }

        private LeafOfAcceptableMeasParams GetLeafOfAcceptableMeasParams()
        {
            var result = new LeafOfAcceptableMeasParams();
            result.Resolutions = ParseLineOfVariantsForParam((int)ServiceFunctionFirstParam.Res);
            result.PulseDurations = ParseLineOfVariantsForParam((int)ServiceFunctionFirstParam.Pulse);

            SetParam((int)ServiceFunctionFirstParam.IsTime, 1);
            result.PeriodsToAverage = ParseLineOfVariantsForParam((int)ServiceFunctionFirstParam.Time);

            SetParam((int)ServiceFunctionFirstParam.IsTime, 0);
            result.MeasCountsToAverage = ParseLineOfVariantsForParam((int)ServiceFunctionFirstParam.Navr);
            return result;
        }

        public void SetMeasurementParametersFromUserInput(UserInputedMeasParams list)
        {
            foreach (var measParam in list.MeasParams)
            {
                SetParam(measParam.Item1, measParam.Item2);
            }
        }
    }
}
