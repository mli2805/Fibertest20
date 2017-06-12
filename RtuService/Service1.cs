using System.ServiceModel;
using System.ServiceProcess;
using System.Threading;
using RtuManagement;

namespace RtuService
{
    public partial class Service1 : ServiceBase
    {
        internal static ServiceHost MyServiceHost;
        private RtuManager _rtuManager;
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            MyServiceHost?.Close();
            MyServiceHost = new ServiceHost(typeof(Service1));
            MyServiceHost.Open();

            _rtuManager = new RtuManager();
            Thread rtuManagerThread = new Thread(_rtuManager.Start);
            rtuManagerThread.Start();
        }

        protected override void OnStop()
        {
            if (MyServiceHost != null)
            {
                MyServiceHost.Close();
                MyServiceHost = null;
            }

            _rtuManager.Stop();
        }
    }
}
