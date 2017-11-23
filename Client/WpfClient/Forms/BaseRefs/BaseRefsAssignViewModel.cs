using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;
using Microsoft.Win32;

namespace Iit.Fibertest.Client
{
    public class BaseRefsAssignViewModel : Screen
    {
        private readonly IniFile _iniFile;
        private Trace _trace;
        private readonly ReadModel _readModel;

        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly IWindowManager _windowManager;
        private readonly SorExt _sorExt;

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
                IsButtonSaveEnabled = true;
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
                IsButtonSaveEnabled = true;
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
                IsButtonSaveEnabled = true;
                NotifyOfPropertyChange();
            }
        }

        private bool _isButtonSaveEnabled;

        public bool IsButtonSaveEnabled
        {
            get { return _isButtonSaveEnabled; }
            set
            {
                if (value == _isButtonSaveEnabled) return;
                _isButtonSaveEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        private string _initialDirectory;
        private string InitialDirectory
        {
            get { return _initialDirectory; }
            set
            {
                if (value == _initialDirectory)
                    return;
                _initialDirectory = value;
                _iniFile.Write(IniSection.Miscellaneous, IniKey.PathToSor, InitialDirectory);
            }
        }

        public BaseRefsAssignViewModel(IniFile iniFile, ReadModel readModel, 
            IWcfServiceForClient c2DWcfManager, IWindowManager windowManager, SorExt sorExt)
        {
            _iniFile = iniFile;
            _readModel = readModel;
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;
            _sorExt = sorExt;
        }

        public void Initialize(Trace trace)
        {
            _trace = trace;
            TraceTitle = _trace.Title;
            TracePortOnRtu = _trace.Port > 0 ? _trace.Port.ToString() : Resources.SID_not_attached;
            PreciseBaseFilename = _trace.PreciseId == Guid.Empty ? "" : _savedInDb;
            FastBaseFilename = _trace.FastId == Guid.Empty ? "" : _savedInDb;
            AdditionalBaseFilename = _trace.AdditionalId == Guid.Empty ? "" : _savedInDb;
                IsButtonSaveEnabled = false;
            RtuTitle = _readModel.Rtus.First(r => r.Id == _trace.RtuId).Title;

            InitialDirectory = _iniFile.Read(IniSection.Miscellaneous, IniKey.PathToSor, @"c:\temp\");
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
            OpenFileDialog dialog = new OpenFileDialog() {Filter = Resources.SID_Reflectogram_files, InitialDirectory = InitialDirectory, };
            if (dialog.ShowDialog() == true)
            {
                PreciseBaseFilename = dialog.FileName;
                InitialDirectory = Path.GetDirectoryName(dialog.FileName);
            }
        }
        public void GetPathToFast()
        {
            OpenFileDialog dialog = new OpenFileDialog() {Filter = Resources.SID_Reflectogram_files, InitialDirectory = InitialDirectory, };
            if (dialog.ShowDialog() == true)
                FastBaseFilename = dialog.FileName;
        }
        public void GetPathToAdditional()
        {
            OpenFileDialog dialog = new OpenFileDialog() {Filter = Resources.SID_Reflectogram_files, InitialDirectory = InitialDirectory, };
            if (dialog.ShowDialog() == true)
                AdditionalBaseFilename = dialog.FileName;
        }

        public void ClearPathToPrecise() { PreciseBaseFilename = ""; }
        public void ClearPathToFast() { FastBaseFilename = ""; }
        public void ClearPathToAdditional() { AdditionalBaseFilename = ""; }

        public async void Save()
        {
            var dto = new AssignBaseRefDto()
                { RtuId = _trace.RtuId, TraceId = _trace.Id, OtauPortDto = _trace.OtauPort, BaseRefs = GetBaseRefChangesList() };
            var result = await _c2DWcfManager.AssignBaseRefAsync(dto); // send to Db and RTU
            if (result.ReturnCode != ReturnCode.BaseRefAssignedSuccessfully)
            {
                var vm = new NotificationViewModel(Resources.SID_Error_,
                    result.ReturnCode.GetLocalizedString(result.ExceptionMessage));
                _windowManager.ShowDialog(vm);
                return;
            }

            var cmd = new AssignBaseRef() { TraceId = _trace.Id, BaseRefs = dto.BaseRefs };
            await _c2DWcfManager.SendCommandAsObj(cmd); // graph

            TryClose();
        }

        public List<BaseRefDto> GetBaseRefChangesList()
        {
            var result = new List<BaseRefDto>();
            if (IsFilenameChanged(PreciseBaseFilename, _trace.PreciseId))
                result.Add(_sorExt.GetBaseRefDto(PreciseBaseFilename, BaseRefType.Precise));
            if (IsFilenameChanged(FastBaseFilename, _trace.FastId))
                result.Add(_sorExt.GetBaseRefDto(FastBaseFilename, BaseRefType.Fast));
            if (IsFilenameChanged(AdditionalBaseFilename, _trace.AdditionalId))
                result.Add(_sorExt.GetBaseRefDto(AdditionalBaseFilename, BaseRefType.Additional));
            return result;
        }


        public void Cancel()
        {
            TryClose();
        }
    }
}
