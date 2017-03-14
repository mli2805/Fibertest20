using System;
using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.TestBench
{
    public class EqItemVm : PropertyChangedBase
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public string Comment { get; set; }
        public string Traces { get; set; }

        public bool IsRemoveEnabled { get; set; }

        private object _command;
        public object Command
        {
            get { return _command; }
            set
            {
                if (Equals(value, _command)) return;
                _command = value;
                NotifyOfPropertyChange();
            }
        }

        // context menu
        public void UpdateEquipment()
        {
            Command = new UpdateEquipment { Id = Id };
        }

        public void RemoveEquipment()
        {
            Command = new RemoveEquipment { Id = Id };
        }
    }
}