using System;
using System.Linq;
using System.Runtime.InteropServices;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.IitOtdrLibrary
{
    public partial class IitOtdrWrapper
    {
        public SetOfAcceptableMeasParams AcceptableMeasParams { get; set; }
        private void InitializeAcceptableMeasParams()
        {
            AcceptableMeasParams = new SetOfAcceptableMeasParams
            {
                Units = GetLineOfVariantsForParam((int) ServiceFunctionFirstParam.Unit),
                Distances = GetLineOfVariantsForParam((int) ServiceFunctionFirstParam.Lmax),
                Resolutions = GetLineOfVariantsForParam((int) ServiceFunctionFirstParam.Res),
                PulseDurations = GetLineOfVariantsForParam((int) ServiceFunctionFirstParam.Pulse)
            };
        }

        public string GetLineOfVariantsForParam(int param)
        {
            int cmd = (int)ServiceFunctionCommand.GetParam;
            int prm1 = param;

            IntPtr unmanagedPointer = IntPtr.Zero;
            int res = ServiceFunction(cmd, ref prm1, ref unmanagedPointer);
            if (res != 0)
                return null;

            return Marshal.PtrToStringAnsi(unmanagedPointer);
        }

        public string[] ParseLineOfVariantsForParam(int paramCode)
        {
            string value = GetLineOfVariantsForParam(paramCode);
            if (value == null)
                return null;

            // if there is only one variant it will be returned without leading slash
            if (value[0] != '/')
                return new[] { value };

            var strs = value.Split('/');
            return strs.Skip(1).ToArray();
        }

        public void SetParam(int param, int indexInLine)
        {
            int cmd = (int)ServiceFunctionCommand.SetParam;
            int prm1 = param;
            IntPtr prm2 = new IntPtr(indexInLine);
            var result = ServiceFunction(cmd, ref prm1, ref prm2);
            if (result != 0)
                _rtuLogger.AppendLine($"Set parameter error={result}!");
        }

        public bool SetMeasurementParametersFromSor(ref IntPtr baseSorData)
        {
            int cmd = (int)ServiceFunctionCommand.SetParamFromSor;
            int reserved = 0;

            var result = ServiceFunction(cmd, ref reserved, ref baseSorData);
            if (result != 0)
                _rtuLogger.AppendLine($"Set parameters from sor error={result}!");
            return result == 0;
        }

        public bool ForceLmaxNs(int lmaxNs)
        {
            int cmd = (int)ServiceFunctionCommand.ParamMeasLmaxSet;
            IntPtr reserved = IntPtr.Zero;
            var result = ServiceFunction(cmd, ref lmaxNs, ref reserved);
            if (result != 1)
                _rtuLogger.AppendLine($"Force Lmax in ns error={result}!");
            return result == 1;
        }

    }
}
