using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class TraceChoiceViewModel : Screen
    {
        public List<Trace> Rows { get; set; }
        public Trace SelectedTrace { get; set; }
        public bool IsAnswerPositive;

        public void Initialize(List<Trace> traces)
        {
            IsAnswerPositive = false;
            Rows = traces;
            SelectedTrace = Rows.First();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Select_trace;
        }

        public void Ok()
        {
            IsAnswerPositive = true;
            TryClose();
        }

        public void Cancel()
        {
            TryClose();
        }

    }
}
