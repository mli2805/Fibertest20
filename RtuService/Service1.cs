using System.ServiceModel;
using System.ServiceProcess;
using Iit.Fibertest.Utils35;

namespace RtuService
{
    public partial class Service1 : ServiceBase
    {
        private Logger35 _rtuLogger; 

        internal static ServiceHost myServiceHost = null;
        public Service1()
        {
            InitializeComponent();

//            _rtuLogger = new Logger35();
//            _rtuLogger.AssignFile("rtu.log");
//            _rtuLogger.AppendLine("Service started.");
        }

        protected override void OnStart(string[] args)
        {
            if (myServiceHost != null)
            {
                myServiceHost.Close();
            }
            myServiceHost = new ServiceHost(typeof(Service1));
            myServiceHost.Open();
        }

        protected override void OnStop()
        {
            if (myServiceHost != null)
            {
                myServiceHost.Close();
                myServiceHost = null;
            }
        }
    }
}
