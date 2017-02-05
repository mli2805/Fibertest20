using System;
using System.Linq;
using AutoMapper;
using Caliburn.Micro;
using Iit.Fibertest.Graph.Commands;

namespace Iit.Fibertest.TestBench
{
    public class RtuUpdateViewModel : Screen
    {
        public Guid Id { get; set; }
        public Guid NodeId { get; set; }
        private readonly GraphVm _graphVm;

        public string Title { get; set; }
        public string Comment { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public UpdateRtu Command { get; set; }

        public RtuUpdateViewModel(Guid nodeId, GraphVm graphVm)
        {
            NodeId = nodeId;
            _graphVm = graphVm;

            Initilize();
        }

        private void Initilize()
        {
            var rtu = _graphVm.Rtus.First(r => r.Node.Id == NodeId);
            Id = rtu.Id;
            var node = rtu.Node;
            Latitude = node.Position.Lat;
            Longitude = node.Position.Lng;
        }

        protected override void OnViewLoaded(object view)
        {
        }

        public void Save()
        {
            IMapper mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingViewModelToCommand>()).CreateMapper();
            Command = mapper.Map<UpdateRtu>(this);
            TryClose();
        }

        public void Cancel()
        {
            TryClose();
        }
    }


}
