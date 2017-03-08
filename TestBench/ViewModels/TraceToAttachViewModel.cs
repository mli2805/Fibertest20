using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.TestBench
{
    public class TraceToAttachViewModel : Screen
    {
        private readonly int _portNumber;
        private readonly Bus _bus;
        private Trace _selectedTrace;

        public List<Trace> Choices { get; set; }

        public Trace SelectedTrace
        {
            get { return _selectedTrace; }
            set
            {
                if (Equals(value, _selectedTrace)) return;
                _selectedTrace = value;
                NotifyOfPropertyChange();
            }
        }

        public TraceToAttachViewModel(Guid rtuId, int portNumber, ReadModel readModel, Bus bus)
        {
            _portNumber = portNumber;
            _bus = bus;

            Choices = readModel.Traces.Where(t => t.RtuId == rtuId && t.Port < 1).ToList();
            SelectedTrace = Choices.FirstOrDefault();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Not_attached_traces_list;
        }

        public void Attach()
        {
            _bus.SendCommand(new AttachTrace() {Port = _portNumber, TraceId = SelectedTrace.Id});
            TryClose();
        }

        public void Cancel()
        {
            TryClose();
        }
    }
}