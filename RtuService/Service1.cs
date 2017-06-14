using System.Globalization;
using System.ServiceModel;
using System.ServiceProcess;
using System.Threading;
using Iit.Fibertest.Utils35;
using RtuManagement;
using RtuWcfServiceLibrary;

namespace RtuService
{
    public partial class Service1 : ServiceBase
    {
        internal static ServiceHost MyServiceHost;
        private readonly IniFile _iniFile35;
        private readonly Logger35 _logger35;
        private RtuManager _rtuManager;

        public Service1()
        {
            InitializeComponent();
            _iniFile35 = new IniFile();
            _iniFile35.AssignFile("rtu.ini");
            var cultureString = _iniFile35.Read(IniSection.General, IniKey.Culture, "ru-RU");
            
            _logger35 = new Logger35();
            _logger35.AssignFile("rtu.log", cultureString);

            _logger35.EmptyLine();
            _logger35.EmptyLine('-');
            _logger35.AppendLine("Windows service started.");
        }

        protected override void OnStart(string[] args)
        {
            MyServiceHost?.Close();
            MyServiceHost = new ServiceHost(typeof(RtuWcfService));
            MyServiceHost.Open();

            _rtuManager = new RtuManager(_iniFile35, _logger35);
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