using System;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.WpfClient.ViewModels
{
    public class AssignBaseRefsViewModel : Screen
    {
        private readonly ReadModel _readModel;
        private readonly Aggregate _aggregate;
        private Trace _trace;

        private const string SavedInDb = "Сохранено в БД";

        public string TraceTitle { get; private set; }
        public string RtuTitle { get; private set; }
        public string RtuPort { get; private set; }

        public string PreciseBaseFilename { get; set; }
        public string FastBaseFilename { get; set; }
        public string AdditionalBaseFilename { get; set; }

        public AssignBaseRefsViewModel(Guid traceId, ReadModel readModel, Aggregate aggregate)
        {
            _readModel = readModel;
            _aggregate = aggregate;

            Initialize(traceId);
        }

        private void Initialize(Guid traceId)
        {
            _trace = _readModel.Traces.First(t => t.Id == traceId);
            var rtu = _readModel.FindRtuByTrace(traceId);

            TraceTitle = _trace.Title;
            RtuTitle = rtu.Title;
            RtuPort = _trace.Port.ToString();

            PreciseBaseFilename    = _trace.PreciseId    == Guid.Empty ? "" : SavedInDb;
            FastBaseFilename       = _trace.FastId       == Guid.Empty ? "" : SavedInDb;
            AdditionalBaseFilename = _trace.AdditionalId == Guid.Empty ? "" : SavedInDb;
        }

        public void Save()
        {
            TryClose();
        }

        public void Cancel()
        {
            TryClose();
        }
    }
}
