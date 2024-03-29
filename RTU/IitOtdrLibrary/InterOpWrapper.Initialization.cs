﻿using System;
using System.Runtime.InteropServices;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.IitOtdrLibrary
{
    public partial class InterOpWrapper
    {
        private readonly IMyLog _rtuLogger;
        //EXTERN_C __declspec(dllexport) void DllInit(char* pathDLL, void* logFile, TLenUnit* lenUnit);
        [DllImport("iit_otdr.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "DllInit")]
        public static extern void DllInit(string path, IntPtr logFile, IntPtr lenUnit);

        // EXTERN_C __declspec(dllexport) int InitOTDR(int Type, const char* Name_IP, long Speed_Tport);
        [DllImport("iit_otdr.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "InitOTDR")]
        public static extern int InitOTDR(int type, string ip, int port);

        public InterOpWrapper(IMyLog rtuLogger)
        {
            _rtuLogger = rtuLogger;
        }

        public bool InitDll(string folder)
        {
            string path = folder;
            IntPtr logFile = IntPtr.Zero;
            IntPtr lenUnit = IntPtr.Zero;
            try
            {
                DllInit(path, logFile, lenUnit);
            }
            catch (Exception e)
            {
                _rtuLogger.AppendLine(e.Message);
                return false;
            }
            return true;
        }

        public bool InitOtdr(ConnectionTypes type, string ip, int port)
        {
            int initOtdr;
            try
            {
                initOtdr = InitOTDR((int) type, ip, port);
                SetEqualStepsOfMeasurement();
            }
            catch (ExternalException e)
            {
                _rtuLogger.AppendLine($"External exception code {e.ErrorCode}");
                _rtuLogger.AppendLine($"External exception message {e.Message}");
                return false;
            }
            catch (Exception e)
            {
                _rtuLogger.AppendLine($"Exception: \"{e.Message}\"");
                
                return false;
            }
            if (initOtdr != 0)
                _rtuLogger.AppendLine($"Initialization error: {initOtdr}");
            if (initOtdr == 805)
                _rtuLogger.AppendLine("805 - ERROR_COM_OPEN - check otdr address or reboot rtu");
            return initOtdr == 0;
        }

    }
}
