using System;
using System.Runtime.InteropServices;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.IitOtdrLibrary
{
    public partial class InterOpWrapper
    {
        // EXTERN_C __declspec(dllexport) int ServiceFunction(long cmd, long& prm1, void** prm2);
        [DllImport("iit_otdr.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ServiceFunction")]
        public static extern int ServiceFunction(int cmd, ref int prm1, ref IntPtr prm2);


        public string GetOtdrInfo(int infoType, IntPtr otdrInfo)
        {
            int cmd = (int) ServiceFunctionCommand.Getotdrinfo;

            var result = ServiceFunction(cmd, ref infoType, ref otdrInfo);
            if (result == 0) 
                return Marshal.PtrToStringAnsi(otdrInfo);

            _rtuLogger.AppendLine($"Get OTDR info error ={result}!");
            return "";
        }

        public bool SetBaseForComparison(IntPtr baseSorData)
        {
            int cmd = (int)ServiceFunctionCommand.Setbase;
            int reserved = 0;

            var result = ServiceFunction(cmd, ref reserved, ref baseSorData);
            if (result != 0)
                _rtuLogger.AppendLine($"Set base for comparison error={result}!");
            return result == 0;
        }

        public ComparisonReturns CompareActiveLevel(IntPtr measSorData)
        {
            int cmd = (int)ServiceFunctionCommand.MonitorEvents;
            int includeBase = 0;

            var result = (ComparisonReturns)ServiceFunction(cmd, ref includeBase, ref measSorData);
            return result;
        }

        public bool MakeAutoAnalysis(ref IntPtr sorData)
        {
            int cmd = (int)ServiceFunctionCommand.Auto;
            int reserved = 0;

            var result = ServiceFunction(cmd, ref reserved, ref sorData);
            if (result != 0)
                _rtuLogger.AppendLine($"MakeAutoAnalysis error={result}!");
            return result == 0;
        }

      
    }
}
