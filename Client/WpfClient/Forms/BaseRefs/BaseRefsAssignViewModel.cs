﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

using Microsoft.Win32;

namespace Iit.Fibertest.Client
{
    public class BaseRefsAssignViewModel : Screen
    {
        private readonly IniFile _iniFile;
        private Trace _trace;
        private readonly Model _readModel;
        private readonly CurrentUser _currentUser;

        private readonly IWcfServiceCommonC2D _c2RWcfManager;
        private readonly CurrentGis _currentGis;
        private readonly GraphGpsCalculator _graphGpsCalculator;
        private readonly BaseRefDtoFactory _baseRefDtoFactory;
        private readonly BaseRefMessages _baseRefMessages;


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
            IWcfServiceCommonC2D c2RWcfManager,
            CurrentGis currentGis, GraphGpsCalculator graphGpsCalculator,
            BaseRefDtoFactory baseRefDtoFactory, BaseRefMessages baseRefMessages)
        {
            _iniFile = iniFile;
            _readModel = readModel;
            _currentUser = currentUser;
            _c2RWcfManager = c2RWcfManager;
            _currentGis = currentGis;
            _graphGpsCalculator = graphGpsCalculator;
            _baseRefDtoFactory = baseRefDtoFactory;
            _baseRefMessages = baseRefMessages;
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


            // if InitialDirectory for OpenFileDialog does not exist:
            //   when drive in InitialDirectory exists - it's ok - will be used previous path from Windows
            //   but if even drive does not exist will be thrown exception
            var pathToSor = FileOperations.GetParentFolder(AppDomain.CurrentDomain.BaseDirectory) + @"\tmp";

            InitialDirectory = _iniFile.Read(IniSection.Miscellaneous, IniKey.PathToSor, pathToSor);
            if (!Directory.Exists(InitialDirectory))
            {
                InitialDirectory = pathToSor;
                _iniFile.Write(IniSection.Miscellaneous, IniKey.PathToSor, InitialDirectory);
            }
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
            var dto = PrepareDto(_trace);
            if (dto.BaseRefs.Any() && IsDistanceLengthAcceptable(dto, _trace))
            {
                var result = await _c2RWcfManager.AssignBaseRefAsync(dto); // send to Db and RTU
                if (result.ReturnCode != ReturnCode.BaseRefAssignedSuccessfully)
                    _baseRefMessages.Display(result, _trace);
                else
                    TryClose();
            }
            IsEditEnabled = true;
        }

        private bool IsDistanceLengthAcceptable(AssignBaseRefsDto dto, Trace trace)
        {
            if (_currentGis.IsWithoutMapMode) return true;

            var precise = dto.BaseRefs.FirstOrDefault(b => b.BaseRefType == BaseRefType.Precise);
            if (precise == null || precise.Id == Guid.Empty) return true;

            var message = SorData.TryGetFromBytes(precise.SorBytes, out var otdrKnownBlocks);
            if (message != "") return true;

            var gpsDistance = $@"{_graphGpsCalculator.CalculateTraceGpsLengthKm(trace):#,0.##}";
            var opticalLength = $@"{otdrKnownBlocks.GetTraceLengthKm():#,0.##}";
            return _baseRefMessages.IsLengthDifferenceAcceptable(gpsDistance, opticalLength);
        }

        public AssignBaseRefsDto PrepareDto(Trace trace)
        {
            var rtu = _readModel.Rtus.FirstOrDefault(r => r.Id == trace.RtuId);
            if (rtu == null) return null;
            var dto = new AssignBaseRefsDto()
            {
                RtuId = trace.RtuId,
                RtuMaker = rtu.RtuMaker,
                OtdrId = rtu.OtdrId,
                TraceId = trace.TraceId,
                OtauPortDto = trace.OtauPort,
                BaseRefs = new List<BaseRefDto>(),
                DeleteOldSorFileIds = new List<int>()
            };

            var baseRefs = new List<BaseRefDto>();
            if (IsFilenameChanged(PreciseBaseFilename, trace.PreciseId))
            {
                var baseRefDto = _baseRefDtoFactory.CreateFromFile(PreciseBaseFilename,
                    BaseRefType.Precise, _currentUser.UserName);
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
