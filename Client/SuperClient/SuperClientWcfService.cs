using System.ServiceModel;
using System.Threading.Tasks;
using Iit.Fibertest.SuperClientWcfServiceInterface;

namespace Iit.Fibertest.SuperClient
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class SuperClientWcfService : IWcfServiceInSuperClient
    {
        private readonly ChildStarter _childStarter;
        public SuperClientWcfService(ChildStarter childStarter)
        {
            _childStarter = childStarter;
        }

        public Task<int> ClientLoaded(int postfix)
        {
            _childStarter.PlaceFtClientOnPanel(postfix);
            return Task.FromResult(0);
        }
    }
}