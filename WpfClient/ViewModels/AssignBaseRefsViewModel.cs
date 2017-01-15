using System;
using System.IO;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;

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

        private void SendAssingBaseRef(string filename, BaseRefType type)
        {
            _aggregate.When(new AssignBaseRef()
            {
                Id = filename != "" ? Guid.NewGuid() : Guid.Empty, TraceId = _trace.Id, Type = type, Content = filename != "" ? File.ReadAllBytes(filename) : null
            } );
        }

        private bool IsFilenameChanged(string filename, Guid previousBaseRefId)
        {
            return ((filename != "" && filename != SavedInDb) || (filename == "" && previousBaseRefId != Guid.Empty));
        }

        public void Save()
        {
            if (IsFilenameChanged(PreciseBaseFilename, _trace.PreciseId))
                SendAssingBaseRef(PreciseBaseFilename, BaseRefType.Precise);
            if (IsFilenameChanged(FastBaseFilename, _trace.FastId))
                SendAssingBaseRef(FastBaseFilename, BaseRefType.Fast);
            if (IsFilenameChanged(AdditionalBaseFilename, _trace.AdditionalId))
                SendAssingBaseRef(AdditionalBaseFilename, BaseRefType.Additional);
            TryClose();
        }

        public void Cancel()
        {
            TryClose();
        }
    }
}
