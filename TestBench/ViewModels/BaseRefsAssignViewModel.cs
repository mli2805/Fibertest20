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
        private TraceVm _traceVm;
        private RtuVm _rtuVm;

        private const string SavedInDb = "Сохранено в БД";

        public string TraceTitle { get; private set; }
        public string RtuTitle { get; private set; }
        public string RtuPort { get; private set; }

        public string PreciseBaseFilename { get; set; }
        public string FastBaseFilename { get; set; }
        public string AdditionalBaseFilename { get; set; }

        public AssignBaseRef Command { get; set; }

        public BaseRefsAssignViewModel(TraceVm traceVm, RtuVm rtuVm)
        {
            _traceVm = traceVm;
            _rtuVm = rtuVm;

            Initialize();
        }

        private void Initialize()
        {
            TraceTitle = _traceVm.Title;
            RtuTitle = _rtuVm.Title;
            RtuPort = _traceVm.Port.ToString();

            PreciseBaseFilename = _traceVm.PreciseId == Guid.Empty ? "" : SavedInDb;
            FastBaseFilename = _traceVm.FastId == Guid.Empty ? "" : SavedInDb;
            AdditionalBaseFilename = _traceVm.AdditionalId == Guid.Empty ? "" : SavedInDb;
        }

        private void SendAssingBaseRef(string filename, BaseRefType type)
        {
            var guid = filename != "" ? Guid.NewGuid() : Guid.Empty;
            Command.Ids.Add(type, guid);
            var content = filename != "" ? File.ReadAllBytes(filename) : null;
            Command.Contents.Add(guid, content);
        }

        private bool IsFilenameChanged(string filename, Guid previousBaseRefId)
        {
            return ((filename != "" && filename != SavedInDb) || (filename == "" && previousBaseRefId != Guid.Empty));
        }

        public void Save()
        {
            Command = new AssignBaseRef() {TraceId = _traceVm.Id};
            if (IsFilenameChanged(PreciseBaseFilename, _traceVm.PreciseId))
                SendAssingBaseRef(PreciseBaseFilename, BaseRefType.Precise);
            if (IsFilenameChanged(FastBaseFilename, _traceVm.FastId))
                SendAssingBaseRef(FastBaseFilename, BaseRefType.Fast);
            if (IsFilenameChanged(AdditionalBaseFilename, _traceVm.AdditionalId))
                SendAssingBaseRef(AdditionalBaseFilename, BaseRefType.Additional);

            TryClose();
        }

        public void Cancel()
        {
            TryClose();
        }
    }
}
