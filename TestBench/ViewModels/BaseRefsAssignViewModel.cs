using System;
using System.IO;
using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.TestBench
{
    public class BaseRefsAssignViewModel : Screen
    {
        private readonly Trace _trace;

        private const string SavedInDb = "Сохранено в БД";

        public string RtuTitle { get; private set; }

        public string TraceTitle { get; private set; }
        public string TracePortOnRtu { get; private set; }
        public string PreciseBaseFilename { get; set; }
        public string FastBaseFilename { get; set; }
        public string AdditionalBaseFilename { get; set; }

        public AssignBaseRef Command { get; set; }

        public BaseRefsAssignViewModel(Trace trace, string rtuTitle)
        {
            RtuTitle = rtuTitle;
            _trace = trace;

            Initialize();
        }

        private void Initialize()
        {
            TraceTitle = _trace.Title;
            TracePortOnRtu = _trace.Port.ToString();
            PreciseBaseFilename = _trace.PreciseId == Guid.Empty ? "" : SavedInDb;
            FastBaseFilename = _trace.FastId == Guid.Empty ? "" : SavedInDb;
            AdditionalBaseFilename = _trace.AdditionalId == Guid.Empty ? "" : SavedInDb;
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
            Command = new AssignBaseRef() {TraceId = _trace.Id};
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
