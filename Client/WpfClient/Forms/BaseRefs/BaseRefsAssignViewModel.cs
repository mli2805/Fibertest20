using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;
using Iit.Fibertest.WpfCommonViews;

using Microsoft.Win32;

namespace Iit.Fibertest.Client
{
    public class BaseRefsAssignViewModel : Screen
    {
        private readonly IniFile _iniFile;
        private Trace _trace;
        private readonly Model _readModel;
        private readonly CurrentUser _currentUser;

        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly IWindowManager _windowManager;
        private readonly BaseRefDtoFactory _baseRefDtoFactory;
        private readonly BaseRefsChecker _baseRefsChecker;

 
        private readonly string _savedInDb = Resources.SID_Saved_in_DB;
        private string _lastChosenFile;

        public string RtuTitle { get; private set; }

        public string TraceTitle { get; private set; }
        public string TracePortOnRtu { get; private set; }

        private string _preciseBaseFilename;
        public string PreciseBaseFilename
        {
            get => _preciseBaseFilename;
            set
            {
                if (value == _preciseBaseFilename) return;
                _preciseBaseFilename = value;
                IsButtonSaveEnabled = true;
                NotifyOfPropertyChange();
            }
        }

        private string _fastBaseFilename;
        public string FastBaseFilename
        {
            get => _fastBaseFilename;
            set
            {
                if (value == _fastBaseFilename) return;
                _fastBaseFilename = value;
                IsButtonSaveEnabled = true;
                NotifyOfPropertyChange();
            }
        }

        private string _additionalBaseFilename;
        public string AdditionalBaseFilename
        {
            get => _additionalBaseFilename;
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
            get => _isButtonSaveEnabled;
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
            get => _initialDirectory;
            set
            {
                if (value == _initialDirectory)
                    return;
                _initialDirectory = value;
                _iniFile.Write(IniSection.Miscellaneous, IniKey.PathToSor, InitialDirectory);
            }
        }

        private bool _isEditEnabled;
        public bool IsEditEnabled
        {
            get => _isEditEnabled;
            set
            {
                if (value == _isEditEnabled) return;
                _isEditEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public BaseRefsAssignViewModel(IniFile iniFile, Model readModel, CurrentUser currentUser,
            IWcfServiceForClient c2DWcfManager, IWindowManager windowManager,
            BaseRefDtoFactory baseRefDtoFactory, BaseRefsChecker baseRefsChecker)
        {
            _iniFile = iniFile;
            _readModel = readModel;
            _currentUser = currentUser;
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;
            _baseRefDtoFactory = baseRefDtoFactory;
            _baseRefsChecker = baseRefsChecker;
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
            IsEditEnabled = true;
            RtuTitle = _readModel.Rtus.First(r => r.Id == _trace.RtuId).Title;

            InitialDirectory = _iniFile.Read(IniSection.Miscellaneous, IniKey.PathToSor, @"c:\temp\");
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Base_refs_assignment;
        }

        private bool IsFilenameChanged(string filename, Guid previousBaseRefId)
        {
            return (filename != "" && filename != _savedInDb) || (filename == "" && previousBaseRefId != Guid.Empty);
        }

        public void GetPathToPrecise()
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Filter = Resources.SID_Reflectogram_files, 
                InitialDirectory = InitialDirectory,
                FileName = _lastChosenFile,
            };
            if (dialog.ShowDialog() == true)
            {
                PreciseBaseFilename = dialog.FileName;
                InitialDirectory = Path.GetDirectoryName(dialog.FileName);
                _lastChosenFile = Path.GetFileName(dialog.FileName);
            }
        }
        public void GetPathToFast()
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Filter = Resources.SID_Reflectogram_files, 
                InitialDirectory = InitialDirectory,
                FileName = _lastChosenFile,
            };
            if (dialog.ShowDialog() == true)
            {
                FastBaseFilename = dialog.FileName;
                InitialDirectory = Path.GetDirectoryName(dialog.FileName);
                _lastChosenFile = Path.GetFileName(dialog.FileName);
            }
        }
        public void GetPathToAdditional()
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Filter = Resources.SID_Reflectogram_files, 
                InitialDirectory = InitialDirectory,
                FileName = _lastChosenFile,
            };
            if (dialog.ShowDialog() == true)
            {
                AdditionalBaseFilename = dialog.FileName;
                InitialDirectory = Path.GetDirectoryName(dialog.FileName);
                _lastChosenFile = Path.GetFileName(dialog.FileName);
            }
        }

        public void ClearPathToPrecise() { PreciseBaseFilename = ""; }
        public void ClearPathToFast() { FastBaseFilename = ""; }
        public void ClearPathToAdditional() { AdditionalBaseFilename = ""; }

        public async Task Save()
        {
            IsEditEnabled = false;
            var result = await SavingProcess();
            IsEditEnabled = true;
            if (result) TryClose();
        }

        private async Task<bool> SavingProcess()
        {
            var dto = PrepareDto(_trace);
            if (!dto.BaseRefs.Any())
                return false;

            if (!_baseRefsChecker.IsBaseRefsAcceptable(dto.BaseRefs, _trace))
                return false;

            var result = await _c2DWcfManager.AssignBaseRefAsync(dto); // send to Db and RTU
            if (result.ReturnCode != ReturnCode.BaseRefAssignedSuccessfully)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, result.ReturnCode.GetLocalizedString(result.ExceptionMessage));
                _windowManager.ShowDialogWithAssignedOwner(vm);
                return false;
            }

            return true;
        }

        public AssignBaseRefsDto PrepareDto(Trace trace)
        {
            var rtu = _readModel.Rtus.FirstOrDefault(r => r.Id == trace.RtuId);
            if (rtu == null) return null;
            var dto = new AssignBaseRefsDto()
            { 
                RtuId = trace.RtuId, 
                RtuMaker = rtu.RtuMaker,
                TraceId = trace.TraceId, 
                OtauPortDto = trace.OtauPort, 
                BaseRefs = new List<BaseRefDto>(), 
                DeleteOldSorFileIds = new List<int>()
            };

            var baseRefs = new List<BaseRefDto>();
            if (IsFilenameChanged(PreciseBaseFilename, trace.PreciseId))
            {
                var baseRefDto = _baseRefDtoFactory.CreateFromFile(PreciseBaseFilename, BaseRefType.Precise, _currentUser.UserName);
                if (trace.PreciseId != Guid.Empty)
                    dto.DeleteOldSorFileIds.Add(_readModel.BaseRefs.First(b => b.Id == trace.PreciseId).SorFileId);
                baseRefs.Add(baseRefDto);
            }

            if (IsFilenameChanged(FastBaseFilename, trace.FastId))
            {
                var baseRefDto = _baseRefDtoFactory.CreateFromFile(FastBaseFilename, BaseRefType.Fast, _currentUser.UserName);
                if (trace.FastId != Guid.Empty)
                    dto.DeleteOldSorFileIds.Add(_readModel.BaseRefs.First(b => b.Id == trace.FastId).SorFileId);
                baseRefs.Add(baseRefDto);
            }

            if (IsFilenameChanged(AdditionalBaseFilename, trace.AdditionalId))
            {
                var baseRefDto = _baseRefDtoFactory.CreateFromFile(AdditionalBaseFilename, BaseRefType.Additional, _currentUser.UserName);
                if (trace.AdditionalId != Guid.Empty)
                    dto.DeleteOldSorFileIds.Add(_readModel.BaseRefs.First(b => b.Id == trace.AdditionalId).SorFileId);
                baseRefs.Add(baseRefDto);
            }

            dto.BaseRefs = baseRefs;
            return dto;
        }

        public void Cancel()
        {
            TryClose();
        }
    }
}
