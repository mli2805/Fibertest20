using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using GMap.NET;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class OneLandmarkViewModel : PropertyChangedBase, IDataErrorInfo
    {
        private readonly CurrentUser _currentUser;
        private readonly IWindowManager _windowManager;

        private GpsInputSmallViewModel _gpsInputSmallViewModel;
        public GpsInputSmallViewModel GpsInputSmallViewModel
        {
            get => _gpsInputSmallViewModel;
            set
            {
                if (Equals(value, _gpsInputSmallViewModel)) return;
                _gpsInputSmallViewModel = value;
                NotifyOfPropertyChange();
            }
        }

        private EquipmentTypeComboItem _selectedEquipmentTypeItem;
        public EquipmentTypeComboItem SelectedEquipmentTypeItem
        {
            get => _selectedEquipmentTypeItem;
            set
            {
                if (Equals(value, _selectedEquipmentTypeItem) || value == null) return;
                _selectedEquipmentTypeItem = value;
                LandmarkUnderWork.EquipmentType = value.Type;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(IsLeftCableReserveEnabled));
                NotifyOfPropertyChange(nameof(IsRightCableReserveEnabled));
            }
        }

        public bool IsLeftCableReserveEnabled => _selectedEquipmentTypeItem.Type.LeftCableReserveEnabled();
        public bool IsRightCableReserveEnabled => _selectedEquipmentTypeItem.Type.RightCableReserveEnabled();

        private Landmark _landmarkUnderWork;
        public Landmark LandmarkUnderWork
        {
            get => _landmarkUnderWork;
            set
            {
                if (value == null) return;
                _landmarkUnderWork = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(HasPrivileges));
            }
        }


        private List<EquipmentTypeComboItem> _comboItems;
        public List<EquipmentTypeComboItem> ComboItems
        {
            get => _comboItems;
            set
            {
                if (Equals(value, _comboItems)) return;
                _comboItems = value;
                NotifyOfPropertyChange();
            }
        }

        private List<EquipmentTypeComboItem> GetItems(EquipmentType type)
        {
            if (type == EquipmentType.Rtu) return new List<EquipmentTypeComboItem>
                { new EquipmentTypeComboItem(EquipmentType.Rtu) };
            if (type == EquipmentType.EmptyNode) return new List<EquipmentTypeComboItem>
                { new EquipmentTypeComboItem(EquipmentType.EmptyNode) };
            return new List<EquipmentTypeComboItem>
            {
                new EquipmentTypeComboItem(EquipmentType.Closure),
                new EquipmentTypeComboItem(EquipmentType.Cross),
                new EquipmentTypeComboItem(EquipmentType.Terminal),
                new EquipmentTypeComboItem(EquipmentType.CableReserve),
                new EquipmentTypeComboItem(EquipmentType.Other)
            };
        }

        public bool HasPrivileges => _currentUser.Role <= Role.Root && LandmarkUnderWork.EquipmentType != EquipmentType.Rtu;

        public Visibility GisVisibility { get; set; }

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

        private PointLatLng _originalPosition;
        private readonly int _maxCableReserve;

        public OneLandmarkViewModel(IniFile iniFile, CurrentUser currentUser, CurrentGis currentGis,
             GpsInputSmallViewModel gpsInputSmallViewModel, IWindowManager windowManager)
        {
            IsEditEnabled = true;
            GisVisibility = currentGis.IsGisOn ? Visibility.Visible : Visibility.Collapsed;
            _currentUser = currentUser;
            _windowManager = windowManager;
            GpsInputSmallViewModel = gpsInputSmallViewModel;
            _maxCableReserve = iniFile.Read(IniSection.Miscellaneous, IniKey.MaxCableReserve, 200);
        }

        public void Initialize(Landmark selectedLandmark)
        {
            LandmarkUnderWork = selectedLandmark;
            LeftCableReserveLocal = selectedLandmark.LeftCableReserve.ToString(CultureInfo.InvariantCulture);
            RightCableReserveLocal = selectedLandmark.RightCableReserve.ToString(CultureInfo.InvariantCulture);
            UserInputLengthLocal = selectedLandmark.IsUserInput 
                ? selectedLandmark.UserInputLength.ToString(CultureInfo.InvariantCulture) : "";

            GpsInputSmallViewModel.Initialize(selectedLandmark.GpsCoors);
            _originalPosition = selectedLandmark.GpsCoors;

            ComboItems = GetItems(selectedLandmark.EquipmentType);
            SelectedEquipmentTypeItem = ComboItems.First(i => i.Type == selectedLandmark.EquipmentType);
        }

        public void RestoreCoordinates()
        {
            GpsInputSmallViewModel.Initialize(_originalPosition);
        }

        public void ClearUserLength()
        {
            UserInputLengthLocal = "";
        }

        public Landmark GetLandmark()
        {
            var errorMessage = GpsInputSmallViewModel.TryGetPoint(out PointLatLng position);
            if (errorMessage != null)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, errorMessage);
                _windowManager.ShowDialogWithAssignedOwner(vm);
                return null;
            }

            if (UserInputLengthLocal == "")
            {
                LandmarkUnderWork.IsUserInput = false;
                LandmarkUnderWork.UserInputLength = 0;
            }
            else
            {
                if (!double.TryParse(UserInputLengthLocal, out double len))
                {
                    var vm = new MyMessageBoxViewModel(MessageType.Error,
                        Resources.SID_User_input_length_should_be_a_number_or_an_empty_string);
                    _windowManager.ShowDialogWithAssignedOwner(vm);
                    return null;
                }

                LandmarkUnderWork.IsUserInput = true;
                LandmarkUnderWork.UserInputLength = len;
            }

            if (LeftCableReserveLocal == "" || !int.TryParse(LeftCableReserveLocal, out int left))
                LandmarkUnderWork.LeftCableReserve = 0;
            else 
                LandmarkUnderWork.LeftCableReserve = left;
            if (RightCableReserveLocal == "" || !int.TryParse(RightCableReserveLocal, out int right))
                LandmarkUnderWork.RightCableReserve = 0;
            else
                LandmarkUnderWork.RightCableReserve = right;

            LandmarkUnderWork.GpsCoors = position;
            return LandmarkUnderWork.Clone();
        }

        private string _userInputLengthLocal;
        public string UserInputLengthLocal
        {
            get => _userInputLengthLocal;
            set
            {
                if (value.Equals(_userInputLengthLocal)) return;
                _userInputLengthLocal = value;
                NotifyOfPropertyChange();
            }
        }

        private string _leftCableReserveLocal;
        public string LeftCableReserveLocal
        {
            get => _leftCableReserveLocal;
            set
            {
                if (value == _leftCableReserveLocal) return;
                _leftCableReserveLocal = value;
                NotifyOfPropertyChange();
            }
        }

        private string _rightCableReserveLocal;
        public string RightCableReserveLocal
        {
            get => _rightCableReserveLocal;
            set
            {
                if (value == _rightCableReserveLocal) return;
                _rightCableReserveLocal = value;
                NotifyOfPropertyChange();
            }
        }

        public string this[string columnName]
        {
            get
            {
                var errorMessage = string.Empty;
                switch (columnName)
                {
                    case "LeftCableReserveLocal" :
                        int left = 0;
                        if (_leftCableReserveLocal != "" && !int.TryParse(_leftCableReserveLocal, out left))
                        {
                            errorMessage = @"error";
                        }
                        else if (left < 0 || left > _maxCableReserve)
                        {
                            errorMessage = @"error";
                        }
                        break;
                   case "RightCableReserveLocal" :
                        int right = 0;
                        if (_rightCableReserveLocal != "" && !int.TryParse(_rightCableReserveLocal, out right))
                        {
                            errorMessage = @"error";
                        }
                        else if (right < 0 || right > _maxCableReserve)
                        {
                            errorMessage = @"error";
                        }
                        break;
                    case "UserInputLengthLocal":
                        if (_userInputLengthLocal == "") return null;
                        if (!double.TryParse(_userInputLengthLocal, out var length))
                        {
                            errorMessage = Resources.SID_Length_should_be_a_number;
                        }
                        else if (length < 1)
                        {
                            errorMessage = @"error";
                        }
                        break;
                }
                return errorMessage;
            }
        }

        public string Error { get; } = null;
    }
}
