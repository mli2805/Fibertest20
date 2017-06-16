using System;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
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
        private RtuManager _rtuManager;
        private readonly string _cultureString;

        public Service1()
        {
            InitializeComponent();
            var iniFile35 = new IniFile();
            iniFile35.AssignFile("RtuService.ini");
            _cultureString = iniFile35.Read(IniSection.General, IniKey.Culture, "ru-RU");
            
            var logger35 = new Logger35();
            logger35.AssignFile("RtuService.log", _cultureString);

            logger35.EmptyLine();
            logger35.EmptyLine('-');
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            logger35.AppendLine($"Windows service started. Process {pid}, thread {tid}");
            logger35.CloseFile();

        }

        protected override void OnStart(string[] args)
        {
            MyServiceHost?.Close();
            MyServiceHost = new ServiceHost(typeof(RtuWcfService));

            //https://stackoverflow.com/questions/381831/can-wcf-service-have-constructors
//            var instanceProvider = new InstanceProviderBehavior<RtuWcfService>(() => new RtuWcfService(_logger35));
//            instanceProvider.AddToAllContracts(MyServiceHost);

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

            var logger35 = new Logger35();
            logger35.AssignFile("RtuService.log", _cultureString);
            logger35.AppendLine("Windows service stopped.");
        }
    }


    public class InstanceProviderBehavior<T> : IInstanceProvider, IContractBehavior
        where T : class
    {
        private readonly Func<T> m_instanceProvider;

        public InstanceProviderBehavior(Func<T> instanceProvider)
        {
            m_instanceProvider = instanceProvider;
        }

        // I think this method is more suitable to be an extension method of ServiceHost.
        // I put it here in order to simplify the code.
        public void AddToAllContracts(ServiceHost serviceHost)
        {
            foreach (var endpoint in serviceHost.Description.Endpoints)
            {
                endpoint.Contract.Behaviors.Add(this);
            }
        }

        #region IInstanceProvider Members

        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            return this.GetInstance(instanceContext);
        }

        public object GetInstance(InstanceContext instanceContext)
        {
            // Create a new instance of T
            return m_instanceProvider.Invoke();
        }

        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
            try
            {
                var disposable = instance as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
            catch { }
        }

        #endregion

        #region IContractBehavior Members

        public void AddBindingParameters(ContractDescription contractDescription, ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
        }

        public void ApplyDispatchBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, DispatchRuntime dispatchRuntime)
        {
            dispatchRuntime.InstanceProvider = this;
        }

        public void Validate(ContractDescription contractDescription, ServiceEndpoint endpoint)
        {
        }

        #endregion
    }
}