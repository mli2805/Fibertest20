using System.ServiceModel;
using System.Threading.Tasks;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.SuperClient
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class SuperClientWcfService : IWcfServiceInSuperClient
    {
        private readonly ChildStarter _childStarter;
        private readonly ServersViewModel _serversViewModel;

        public SuperClientWcfService(ChildStarter childStarter, ServersViewModel serversViewModel)
        {
            _childStarter = childStarter;
            _serversViewModel = serversViewModel;
        }

        public Task<int> ClientLoadingResult(int postfix, bool isLoadedOk, bool isStateOk)
        {
            if (isLoadedOk)
                _childStarter.PlaceFtClientOnPanel(postfix);
            else
                _childStarter.CleanOnLoadingFailed(postfix);

            _serversViewModel.SetConnectionResult(postfix, isLoadedOk, isStateOk);
            return Task.FromResult(0);
        }

        public Task<int> NotifyConnectionBroken(int postfix)
        {
            _serversViewModel.CleanBrokenConnection(postfix);
            return Task.FromResult(0);
        }

        public Task<int> ClientClosed(int postfix)
        {
            _serversViewModel.SetServerIsClosed(postfix);
            return Task.FromResult(0);
        }

        public Task<int> SetSystemState(int postfix, bool isStateOk)
        {
            //_childStarter.PlaceFtClientOnPanel(postfix);
            _serversViewModel.SetSystemState(postfix, isStateOk);
            return Task.FromResult(0);
        }

        public Task<int> SwitchOntoSystem(int postfix)
        {
            _serversViewModel.ChangeSelectedClient(postfix);
            return Task.FromResult(0);
        }
    }
}