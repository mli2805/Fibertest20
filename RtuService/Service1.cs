using System;
using System.Globalization;
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

            //https://stackoverflow.com/questions/381831/can-wcf-service-have-constructors
//            var instanceProvider = new InstanceProviderBehavior<RtuWcfService>(() => new RtuWcfService(_logger35));
//            instanceProvider.AddToAllContracts(MyServiceHost);

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

            _logger35.AppendLine("Windows service stopped.");
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