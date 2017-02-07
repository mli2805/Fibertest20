using System;
using System.IO;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;

namespace Iit.Fibertest.TestBench
{
    public class BaseRefsAssignViewModel : Screen
    {
        private readonly ReadModel _readModel;
        private Trace _trace;

        private const string SavedInDb = "Сохранено в БД";

        public string TraceTitle { get; private set; }
        public string RtuTitle { get; private set; }
        public string RtuPort { get; private set; }

        public string PreciseBaseFilename { get; set; }
        public string FastBaseFilename { get; set; }
        public string AdditionalBaseFilename { get; set; }

        public AssignBaseRef Command { get; set; }

        public BaseRefsAssignViewModel(Guid traceId, ReadModel readModel)
        {
            _readModel = readModel;

            Initialize(traceId);
        }

        private void Initialize(Guid traceId)
        {
            _trace = _readModel.Traces.First(t => t.Id == traceId);
            var rtu = _readModel.FindRtuByTrace(traceId);

            TraceTitle = _trace.Title;
            RtuTitle = rtu.Title;
            RtuPort = _trace.Port.ToString();

            PreciseBaseFilename = _trace.PreciseId == Guid.Empty ? "" : SavedInDb;
            FastBaseFilename = _trace.FastId == Guid.Empty ? "" : SavedInDb;
            AdditionalBaseFilename = _trace.AdditionalId == Guid.Empty ? "" : SavedInDb;
        }

        private void SendAssingBaseRef(ref AssignBaseRef cmd, string filename, BaseRefType type)
        {
            switch (type)
            {
                case BaseRefType.Precise:
                    cmd.PreciseId = filename != "" ? Guid.NewGuid() : Guid.Empty;
                    cmd.PreciseContent = filename != "" ? File.ReadAllBytes(filename) : null;
                    break;
                case BaseRefType.Fast:
                    cmd.FastId = filename != "" ? Guid.NewGuid() : Guid.Empty;
                    cmd.FastContent = filename != "" ? File.ReadAllBytes(filename) : null;
                    break;
                case BaseRefType.Additional:
                    cmd.AdditionalId = filename != "" ? Guid.NewGuid() : Guid.Empty;
                    cmd.AdditionalContent = filename != "" ? File.ReadAllBytes(filename) : null;
                    break;
            }
        }

        private bool IsFilenameChanged(string filename, Guid previousBaseRefId)
        {
            return ((filename != "" && filename != SavedInDb) || (filename == "" && previousBaseRefId != Guid.Empty));
        }

        public void Save()
        {
            var cmd = new AssignBaseRef() { TraceId = _trace.Id };
            var flag = false;

            if (IsFilenameChanged(PreciseBaseFilename, _trace.PreciseId))
            {
                SendAssingBaseRef(ref cmd, PreciseBaseFilename, BaseRefType.Precise);
                flag = true;
            }
            if (IsFilenameChanged(FastBaseFilename, _trace.FastId))
            {
                SendAssingBaseRef(ref cmd, FastBaseFilename, BaseRefType.Fast);
                flag = true;
            }
            if (IsFilenameChanged(AdditionalBaseFilename, _trace.AdditionalId))
            {
                SendAssingBaseRef(ref cmd, AdditionalBaseFilename, BaseRefType.Additional);
                flag = true;
            }

            if (flag)
                Command = cmd;

            TryClose();
        }

        public void Cancel()
        {
            TryClose();
        }
    }
}
