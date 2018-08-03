namespace Iit.Fibertest.UtilsLib
{
    public class MikrotikInBop
    {
        private IMyLog _rtuLogFile;
        private string _ip;
        private readonly int _connectionTimeout;
        private Mikrotik _mikrotik;

        public MikrotikInBop(IMyLog logFile, string ip, int connectionTimeout)
        {
            _rtuLogFile = logFile;
            _ip = ip;
            _connectionTimeout = connectionTimeout;
        }

        public bool Connect()
        {
            _rtuLogFile.AppendLine($"Connect Mikrotik {_ip} started...");
            _mikrotik = new Mikrotik(_ip, _connectionTimeout);
            if (!_mikrotik.IsAvailable)
            {
                _rtuLogFile.AppendLine($"Couldn't establish tcp connection with Mikrotik {_ip}");
                return false;
            }
            if (!_mikrotik.Login("admin", ""))
            {
                _rtuLogFile.AppendLine($"Could not log in Mikrotik {_ip}");
                _mikrotik.Close();
                return false;
            }
            return true;
        }

        public void Reboot()
        {
            _rtuLogFile.AppendLine($"Reboot Mikrotik {_ip} started...");
            _mikrotik.Send("/system/reboot", true);
            _mikrotik.Read();
            _mikrotik.Close();
        }

    }
}
