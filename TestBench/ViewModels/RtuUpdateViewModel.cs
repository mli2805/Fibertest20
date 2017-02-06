using System;
using System.Linq;
using AutoMapper;
using Caliburn.Micro;
using Iit.Fibertest.Graph.Commands;

namespace Iit.Fibertest.TestBench
{
    public class RtuUpdateViewModel : Screen
    {
        private readonly Guid _nodeId;
        private readonly GraphVm _graphVm;

        public string Title { get; set; }
        public string Comment { get; set; }

        public GpsInputViewModel GpsInputViewModel { get; set; }

        public UpdateRtu Request { get; set; }

        public RtuUpdateViewModel(Guid nodeId, GraphVm graphVm)
        {
            _nodeId = nodeId;
            _graphVm = graphVm;

            Initilize();
        }

        private void Initilize()
        {
            var rtu = _graphVm.Rtus.First(r => r.Node.Id == _nodeId);
            rtu.Title = "bluh-blah";

            var node = rtu.Node;
            GpsInputViewModel = new GpsInputViewModel(GpsInputMode.DegreesAndMinutes, node.Position);

            Title = rtu.Title;
            Comment = rtu.Comment;
        }

        protected override void OnViewLoaded(object view)
        {
        }

        public void Save()
        {
            IMapper mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingViewModelToCommand>()).CreateMapper();
            Request = mapper.Map<UpdateRtu>(this);
            TryClose();
        }

        public void Cancel()
        {
            TryClose();
        }
    }


}
