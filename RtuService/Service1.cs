using System.ServiceModel;
using System.ServiceProcess;

namespace RtuService
{
    public partial class Service1 : ServiceBase
    {
        internal static ServiceHost MyServiceHost;
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            MyServiceHost?.Close();
            MyServiceHost = new ServiceHost(typeof(Service1));
            MyServiceHost.Open();
        }

        protected override void OnStop()
        {
            if (MyServiceHost != null)
            {
                MyServiceHost.Close();
                MyServiceHost = null;
            }
        }
    }
}
