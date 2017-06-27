namespace Iit.Fibertest.Utils35
{
    public class MikrotikInBop
    {
        private Logger35 _rtuLogger35;
        private string _ip;
        private Mikrotik _mikrotik;

        public MikrotikInBop(Logger35 logger35, string ip)
        {
            _rtuLogger35 = logger35;
            _ip = ip;
        }

        public bool Connect()
        {
            _rtuLogger35.AppendLine($"Connect Mikrotik {_ip} started...");
            _mikrotik = new Mikrotik(_ip, 5);
            if (!_mikrotik.IsAvailable)
            {
                _rtuLogger35.AppendLine($"Couldn't establish tcp connection with Mikrotik {_ip}");
                return false;
            }
            if (!_mikrotik.Login(@"admin", ""))
            {
                _rtuLogger35.AppendLine($@"Could not log in Mikrotik {_ip}");
                _mikrotik.Close();
                return false;
            }
            return true;
        }

        public void Reboot()
        {
            _rtuLogger35.AppendLine($"Reboot Mikrotik {_ip} started...");
            _mikrotik.Send(@"/system/reboot", true);
        }

    }
}
