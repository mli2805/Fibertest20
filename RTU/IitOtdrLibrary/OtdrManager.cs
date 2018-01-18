using System;
using System.IO;
using System.Runtime.InteropServices;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.IitOtdrLibrary
{
    public partial class OtdrManager
    {
        private readonly string _iitotdrFolder;
        private readonly IniFile _iniFile;
        private readonly IMyLog _rtuLogger;

        public InterOpWrapper InterOpWrapper { get; set; }
        public bool IsLibraryInitialized;
        public bool IsOtdrConnected;

        public OtdrManager(string iitotdrFolder, IniFile iniFile, IMyLog rtuLogger)
        {
            // service can't use relative path, get it obviously
//            var appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
//            var appDir = Path.GetDirectoryName(appPath);
//            _iitotdrFolder = appDir + "\\" + iitotdrFolder;
            _iitotdrFolder = AppDomain.CurrentDomain.BaseDirectory + iitotdrFolder;

            _iniFile = iniFile;
            _rtuLogger = rtuLogger;
            _rtuLogger.AppendLine("OtdrManager initialized");
        }

        public string LoadDll()
        {
            var dllPath = Path.Combine(_iitotdrFolder, @"iit_otdr.dll");
            var handle = Native.LoadLibrary(dllPath);
            string message;
            if (handle == IntPtr.Zero)
            {
                int errorCode = Marshal.GetLastWin32Error();
                message = $"Failed to load library {dllPath} (code: {errorCode})";
                _rtuLogger.AppendLine(message);
                return message;
            }

            message = $"Library {dllPath} loaded successfully";
            _rtuLogger.AppendLine(message);
            return "";
        }
        public bool InitializeLibrary()
        {
            InterOpWrapper = new InterOpWrapper(_rtuLogger);
            _rtuLogger.AppendLine("Initializing iit_otdr (log, ini, temp and other stuff) ...");
            if (!RestoreEtc())
            {
                _rtuLogger.AppendLine("Etc restore problem.");
                return false;
            }
            IsLibraryInitialized = InterOpWrapper.InitDll(_iitotdrFolder);
            _rtuLogger.AppendLine(IsLibraryInitialized ? "Library initialized successfully!" : "Library initialization failed!");
            return IsLibraryInitialized;
        }

        private bool RestoreEtc()
        {
            var destinationPath = Path.Combine(_iitotdrFolder, @"ETC");
            if (!Directory.Exists(destinationPath))
            {
                _rtuLogger.AppendLine($"Can't work without <{destinationPath}> folder!");
                return false;
            }
            var sourcePath = _iitotdrFolder + "\\ETC_default";
            if (!Directory.Exists(sourcePath))
            {
                _rtuLogger.AppendLine($"Can't work without <{sourcePath}> folder!");
                return false;
            }
            var files = Directory.GetFiles(sourcePath);
            foreach (var file in files)
            {
                var sourceFile = Path.GetFileName(file);
//                if (sourceFile == null)
//                    return false;
                var destFile = Path.Combine(destinationPath, sourceFile);
                File.Copy(file, destFile, true);
            }
            return true;
        }

        public bool ConnectOtdr(string ipAddress)
        {
            _rtuLogger.AppendLine($"Connecting to OTDR {ipAddress}...");
            var tcpPort = _iniFile.Read(IniSection.Charon, IniKey.OtdrPort, 1500);
            IsOtdrConnected = InterOpWrapper.InitOtdr(ConnectionTypes.Tcp, ipAddress, tcpPort);
            _rtuLogger.AppendLine(IsOtdrConnected ? "OTDR connected successfully!" : "OTDR connection failed!");
            if (!IsOtdrConnected)
                LedDisplay.Show(_iniFile, _rtuLogger, LedDisplayCode.ErrorConnectOtdr);
            return IsOtdrConnected;
        }

        public void DisconnectOtdr(string ipAddress)
        {
            _rtuLogger.AppendLine($"Disconnecting OTDR {ipAddress}...");
            if (!InterOpWrapper.InitOtdr(ConnectionTypes.FreePort, ipAddress, 1500))
                return;

            IsOtdrConnected = false;
            _rtuLogger.AppendLine("OTDR disconnected successfully!");
        }

    }
}