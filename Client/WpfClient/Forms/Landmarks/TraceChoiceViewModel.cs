using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class TraceItem
    {
        public Guid TraceId { get; set; }
        public string Title { get; set; }

    }
    public class TraceChoiceViewModel : Screen
    {
        public List<Trace> Rows { get; set; }
        public Trace SelectedTrace { get; set; }
        public bool IsAnswerPositive;

        public void Initialize(List<Trace> traces)
        {
            Rows = traces;
            SelectedTrace = Rows.First();
        }

        public void Ok()
        {
            IsAnswerPositive = true;
            TryClose();
        }

        public void Cancel()
        {
            IsAnswerPositive = false;
            TryClose();
        }

    }
}
