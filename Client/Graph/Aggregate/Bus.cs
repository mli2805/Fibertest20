using System.Threading.Tasks;
using PrivateReflectionUsingDynamic;

namespace Iit.Fibertest.Graph
{
    public sealed class Bus
    {
        private readonly Aggregate _aggregate;

        public Bus(Aggregate aggregate)
        {
            _aggregate = aggregate;
        }

        public Task<string> SendCommand(object cmd)
        {
            // If you have an exception here consider checking then When method to return string
            var result = (string)_aggregate.AsDynamic().When(cmd);
            // TODO code from tick
            return Task.FromResult(result);
        }
    }
}