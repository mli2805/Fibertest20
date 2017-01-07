using System;
using System.ComponentModel;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;

namespace Iit.Fibertest.WpfClient.ViewModels
{
    public sealed class MapViewModel : IDataErrorInfo
    {
        private readonly Aggregate _aggregate;

        public MapViewModel(Aggregate aggregate)
        {
            _aggregate = aggregate;
        }

        public Guid AddNode()
        {
            var newGuid = Guid.NewGuid();
            var cmd = new AddNode
            {
                Id = newGuid
            };

            _aggregate.When(cmd);

            return newGuid;
        }


        public void AddFiber(Guid left, Guid right)
        {
            var newGuid = Guid.NewGuid();
            var cmd = new AddFiber()
            {
                Id = newGuid,
                Node1 = left,
                Node2 = right
            };

            Error = _aggregate.When(cmd);
        }

        public string this[string columnName]
        {
            get { throw new NotImplementedException(); }
        }

        public string Error { get; private set; }
    }
}