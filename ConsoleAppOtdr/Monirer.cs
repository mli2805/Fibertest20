using System.IO;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.Utils35;
using Iit.Fibertest.Utils35.IniFile;

namespace ConsoleAppOtdr
{
    public class Monirer
    {
        private readonly Logger35 _logger35;
        private readonly IniFile _iniFile35;
        private OtdrManager _otdrManager;

        public Monirer(Logger35 logger35, IniFile iniFile35)
        {
            _logger35 = logger35;
            _iniFile35 = iniFile35;
        }

        public bool InitializeOtdr()
        {
            _otdrManager = new OtdrManager(@"OtdrMeasEngine\", _logger35);
            if (_otdrManager.LoadDll() != "")
                return false;

            var otdrAddress = _iniFile35.Read(IniSection.General, IniKey.OtdrIp, "192.168.88.101");
            if (_otdrManager.InitializeLibrary())
                _otdrManager.ConnectOtdr(otdrAddress);
            return _otdrManager.IsOtdrConnected;
        }

        public void MoniPort(int port)
        {
            var basefile = $@"..\PortData\{port}\BaseFast.sor";
            if (!File.Exists(basefile))
            {
                _logger35.AppendLine($"Can't find fast base for port {port}");
                return;
            }
            var baseBytes = File.ReadAllBytes(basefile);
            _otdrManager.MeasureWithBase(baseBytes);
        }

    }
}