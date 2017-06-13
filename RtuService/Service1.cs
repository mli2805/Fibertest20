using System.ServiceModel;
using System.ServiceProcess;
using System.Threading;
using RtuManagement;
using RtuWcfServiceLibrary;

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
            MyServiceHost = new ServiceHost(typeof(RtuWcfService));
            MyServiceHost.Open();

            _rtuManager = new RtuManager();
            Thread rtuManagerThread = new Thread(_rtuManager.Start);
            rtuManagerThread.Start();
        }

        protected override void OnStop()
        {
            _rtuManager.Stop();
            if (MyServiceHost != null)
            {
                MyServiceHost.Close();
                MyServiceHost = null;
            }

        }
    }
}
