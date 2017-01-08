using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;

namespace Iit.Fibertest.WpfClient.ViewModels
{
    public class AddTraceViewModel : Screen
    {
        private readonly ReadModel _readModel;
        private readonly Aggregate _aggregate;
        private readonly List<Guid> _nodes;
        private List<Guid> _equipments = new List<Guid>();
        private string _title;
        private string _comment;

        public string Title
        {
            get { return _title; }
            set
            {
                if (value == _title) return;
                _title = value;
                NotifyOfPropertyChange();
            }
        }

        public string Comment
        {
            get { return _comment; }
            set
            {
                if (value == _comment) return;
                _comment = value;
                NotifyOfPropertyChange();
            }
        }


        public bool IsClosed { get; set; }


        public AddTraceViewModel(ReadModel readModel, Aggregate aggregate, List<Guid> nodes)
        {
            _readModel = readModel;
            _aggregate = aggregate;
            _nodes = nodes;

            IsClosed = false;
        }

        public void Save()
        {
            _aggregate.When(new AddTrace()
            {
                Id = new Guid(),
                Title = _title,
                Nodes = _nodes,
                Equipments = _equipments,
                Comment = _comment
            });

            CloseView();
        }

        public void Cancel()
        {
            CloseView();
        }

        private void CloseView()
        {
            IsClosed = true;
            TryClose();
        }

    }
}
