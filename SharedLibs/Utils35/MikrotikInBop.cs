namespace Iit.Fibertest.Utils35
{
    public class MikrotikInBop
    {
        private LogFile _rtuLogFile;
        private string _ip;
        private Mikrotik _mikrotik;

        public MikrotikInBop(LogFile logFile, string ip)
        {
            _rtuLogFile = logFile;
            _ip = ip;
        }

        public bool Connect()
        {
            _rtuLogFile.AppendLine($"Connect Mikrotik {_ip} started...");
            _mikrotik = new Mikrotik(_ip, 5);
            if (!_mikrotik.IsAvailable)
            {
                _rtuLogFile.AppendLine($"Couldn't establish tcp connection with Mikrotik {_ip}");
                return false;
            }
            if (!_mikrotik.Login(@"admin", ""))
            {
                _rtuLogFile.AppendLine($@"Could not log in Mikrotik {_ip}");
                _mikrotik.Close();
                return false;
            }
            return true;
        }

        public void Reboot()
        {
            _rtuLogFile.AppendLine($"Reboot Mikrotik {_ip} started...");
            _mikrotik.Send(@"/system/reboot", true);
        }

    }
}
