using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfServiceForClientInterface;
using Microsoft.Win32;

namespace Iit.Fibertest.Client
{
    public class BaseRefsAssignViewModel : Screen
    {
        private readonly Trace _trace;
        private readonly ReadModel _readModel;

        private readonly IWcfServiceForClient _c2DWcfManager;

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


        public BaseRefsAssignViewModel(Trace trace, ReadModel readModel, IWcfServiceForClient c2DWcfManager)
        {
            _trace = trace;
            _readModel = readModel;
            _c2DWcfManager = c2DWcfManager;

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
        public async void Save()
        {
            var dto = new AssignBaseRefDto()
                { RtuId = _trace.RtuId, OtauPortDto = _trace.OtauAddress, BaseRefs = GetBaseRefChangesList() };
            var result = await _c2DWcfManager.AssignBaseRefAsync(dto); // send to rtu
            if (!result)
                return;

            var cmd = new AssignBaseRef() { TraceId = _trace.Id, BaseRefs = dto.BaseRefs };
            await _c2DWcfManager.SendCommandAsObj(cmd); // graph

            TryClose();
        }

        public List<BaseRefDto> GetBaseRefChangesList()
        {
            var result = new List<BaseRefDto>();
            if (IsFilenameChanged(PreciseBaseFilename, _trace.PreciseId))
                result.Add(GetBaseRef(PreciseBaseFilename, BaseRefType.Precise));
            if (IsFilenameChanged(FastBaseFilename, _trace.FastId))
                result.Add(GetBaseRef(FastBaseFilename, BaseRefType.Fast));
            if (IsFilenameChanged(AdditionalBaseFilename, _trace.AdditionalId))
                result.Add(GetBaseRef(AdditionalBaseFilename, BaseRefType.Additional));
            return result;
        }


        private BaseRefDto GetBaseRef(string filename, BaseRefType type)
        {
            var guid = filename != "" ? Guid.NewGuid() : Guid.Empty;
            var content = filename != "" ? File.ReadAllBytes(filename) : null;
            return new BaseRefDto() {Id = guid, BaseRefType = type, SorBytes = content};
        }
        public void Cancel()
        {
            TryClose();
        }
    }
}
