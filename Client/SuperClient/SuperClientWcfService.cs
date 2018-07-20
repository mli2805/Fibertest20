using System.ServiceModel;
using System.Threading.Tasks;
using Iit.Fibertest.SuperClientWcfServiceInterface;

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

        public Task<int> ClientLoaded(int postfix)
        {
            _childStarter.PlaceFtClientOnPanel(postfix);
            _serversViewModel.SetServerIsReady(postfix);
            return Task.FromResult(0);
        }

        public Task<int> ClientClosed(int postfix)
        {
            _serversViewModel.SetServerIsClosed(postfix);
            return Task.FromResult(0);
        }
    }
}