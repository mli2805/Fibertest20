using System;
using System.Threading.Tasks;
using Iit.Fibertest.Client;
using Iit.Fibertest.Dto;

namespace Graph.Tests
{
    public class FakeRtuHolder : IRtuHolder
    {
        public Task<bool> SetRtuOccupationState(Guid rtuId, string rtuTitle, RtuOccupation rtuOccupation)
        {
            return Task.FromResult(true);
        }
    }


}
