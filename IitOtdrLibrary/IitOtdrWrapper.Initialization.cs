using System;
using System.Runtime.InteropServices;
using Iit.Fibertest.Utils35;

namespace Iit.Fibertest.IitOtdrLibrary
{
    public partial class IitOtdrWrapper
    {
        private readonly Logger35 _rtuLogger;
        //EXTERN_C __declspec(dllexport) void DllInit(char* pathDLL, void* logFile, TLenUnit* lenUnit);
        [DllImport("iit_otdr.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "DllInit")]
        public static extern void DllInit(string path, IntPtr logFile, IntPtr lenUnit);

        // EXTERN_C __declspec(dllexport) int InitOTDR(int Type, const char* Name_IP, long Speed_Tport);
        [DllImport("iit_otdr.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "InitOTDR")]
        public static extern int InitOTDR(int type, string ip, int port);

        public IitOtdrWrapper(Logger35 rtuLogger)
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
                initOtdr = InitOTDR((int)type, ip, port);
            }
            catch (Exception e)
            {
                _rtuLogger.AppendLine(e.Message);
                return false;
            }
            if (initOtdr != 0)
                _rtuLogger.AppendLine($"Initialization error: {initOtdr}");
            return initOtdr == 0;
        }

    }
}
