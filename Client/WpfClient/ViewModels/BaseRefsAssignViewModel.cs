using System;
using System.IO;
using System.Linq;
using Caliburn.Micro;
using Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Microsoft.Win32;
using WcfServiceForClientLibrary;

namespace Iit.Fibertest.Client
{
    public class BaseRefsAssignViewModel : Screen
    {
        private readonly Trace _trace;
        private readonly ReadModel _readModel;
        private readonly Bus _bus;
        private string _preciseBaseFilename;
        private string _fastBaseFilename;
        private string _additionalBaseFilename;

        private readonly string _savedInDb = Resources.SID_Saved_in_DB;

        public string RtuTitle { get; private set; }

        public string TraceTitle { get; private set; }
        public string TracePortOnRtu { get; private set; }

        public string PreciseBaseFilename
        {
            get { return _preciseBaseFilename; }
            set
            {
                if (value == _preciseBaseFilename) return;
                _preciseBaseFilename = value;
                NotifyOfPropertyChange();
            }
        }

        public string FastBaseFilename
        {
            get { return _fastBaseFilename; }
            set
            {
                if (value == _fastBaseFilename) return;
                _fastBaseFilename = value;
                NotifyOfPropertyChange();
            }
        }

        public string AdditionalBaseFilename
        {
            get { return _additionalBaseFilename; }
            set
            {
                if (value == _additionalBaseFilename) return;
                _additionalBaseFilename = value;
                NotifyOfPropertyChange();
            }
        }

        public AssignBaseRef Command { get; set; }

        public BaseRefsAssignViewModel(Trace trace, ReadModel readModel, Bus bus)
        {
            _trace = trace;
            _readModel = readModel;
            _bus = bus;

            Initialize();
        }

        private void Initialize()
        {
            TraceTitle = _trace.Title;
            TracePortOnRtu = _trace.Port > 0 ? _trace.Port.ToString() : Resources.SID_not_attached;
            PreciseBaseFilename = _trace.PreciseId == Guid.Empty ? "" : _savedInDb;
            FastBaseFilename = _trace.FastId == Guid.Empty ? "" : _savedInDb;
            AdditionalBaseFilename = _trace.AdditionalId == Guid.Empty ? "" : _savedInDb;
            RtuTitle = _readModel.Rtus.First(r => r.Id == _trace.RtuId).Title;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Base_refs_assignment;
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
            return ((filename != "" && filename != _savedInDb) || (filename == "" && previousBaseRefId != Guid.Empty));
        }

        public void GetPathToPrecise()
        {
            //TODO get from inifile
            var initialDirectory = @"c:\temp\";
            OpenFileDialog dialog = new OpenFileDialog() {Filter = Resources.SID_Reflectogram_files, InitialDirectory = initialDirectory, };
            if (dialog.ShowDialog() == true)
                PreciseBaseFilename = dialog.FileName;
        }
        public void GetPathToFast()
        {
            //TODO get from inifile
            var initialDirectory = @"c:\temp\";
            OpenFileDialog dialog = new OpenFileDialog() {Filter = Resources.SID_Reflectogram_files, InitialDirectory = initialDirectory, };
            if (dialog.ShowDialog() == true)
                FastBaseFilename = dialog.FileName;
        }
        public void GetPathToAdditional()
        {
            //TODO get from inifile
            var initialDirectory = @"c:\temp\";
            OpenFileDialog dialog = new OpenFileDialog() {Filter = Resources.SID_Reflectogram_files, InitialDirectory = initialDirectory, };
            if (dialog.ShowDialog() == true)
                AdditionalBaseFilename = dialog.FileName;
        }

        public void ClearPathToPrecise() { PreciseBaseFilename = ""; }
        public void ClearPathToFast() { FastBaseFilename = ""; }
        public void ClearPathToAdditional() { AdditionalBaseFilename = ""; }
        public void Save()
        {
            Command = new AssignBaseRef() {TraceId = _trace.Id};
            if (IsFilenameChanged(PreciseBaseFilename, _trace.PreciseId))
                SendAssingBaseRef(PreciseBaseFilename, BaseRefType.Precise);
            if (IsFilenameChanged(FastBaseFilename, _trace.FastId))
                SendAssingBaseRef(FastBaseFilename, BaseRefType.Fast);
            if (IsFilenameChanged(AdditionalBaseFilename, _trace.AdditionalId))
                SendAssingBaseRef(AdditionalBaseFilename, BaseRefType.Additional);

            _bus.SendCommand(Command);

            TryClose();
        }

        public void Cancel()
        {
            TryClose();
        }
    }
}
