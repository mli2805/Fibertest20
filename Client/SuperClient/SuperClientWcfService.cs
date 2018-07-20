using System;
using System.ServiceModel;
using System.Threading.Tasks;
using Iit.Fibertest.SuperClientWcfServiceInterface;

namespace Iit.Fibertest.SuperClient
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class SuperClientWcfService : IWcfServiceInSuperClient
    {
        public Task<int> ClientLoaded(int postfix)
        {
            Console.WriteLine($@"Client {postfix} loaded.");
            return Task.FromResult(0);
        }
    }
}