using System;
using System.Linq;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class RtuInitializeModel : PropertyChangedBase
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IniFile _iniFile;
        private readonly IWindowManager _windowManager;
        private readonly Model _readModel;

        public string RtuName
        {
            get => _rtuName;
            set
            {
                if (value == _rtuName) return;
                _rtuName = value;
                NotifyOfPropertyChange();
            }
        }

        public string RtuId
        {
            get => _rtuId;
            set
            {
                if (value == _rtuId) return;
                _rtuId = value;
                NotifyOfPropertyChange();
            }
        }

        public OtdrAddressViewModel OtdrAddressViewModel { get; set; } = new OtdrAddressViewModel();
        public RtuIitInfoViewModel IitInfoModel { get; set; } = new RtuIitInfoViewModel();
        public RtuVeexInfoViewModel VeexInfoModel { get; set; } = new RtuVeexInfoViewModel();

        private Rtu _originalRtu;
        public Rtu OriginalRtu
        {
            get => _originalRtu;
            set
            {
                if (Equals(value, _originalRtu)) return;
                _originalRtu = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(OtdrAddress));
            }
        }


        public NetAddressTestViewModel MainChannelTestViewModel { get; set; }
        public bool IsReserveChannelEnabled { get; set; }
        public NetAddressTestViewModel ReserveChannelTestViewModel { get; set; }

        public string OtdrAddress => OriginalRtu.OtdrNetAddress.Ip4Address == @"192.168.88.101" // fake address on screen
            ? OriginalRtu.MainChannel.Ip4Address
            : OriginalRtu.OtdrNetAddress.Ip4Address;

        private Visibility _iitVisibility;
        public Visibility IitVisibility
        {
            get => _iitVisibility;
            set
            {
                if (value == _iitVisibility) return;
                _iitVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        private Visibility _veexVisibility;
        private string _rtuName;
        private string _rtuId;

        public Visibility VeexVisibility
        {
            get => _veexVisibility;
            set
            {
                if (value == _veexVisibility) return;
                _veexVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        public RtuInitializeModel(ILifetimeScope globalScope, IniFile iniFile, 
            IWindowManager windowManager, Model readModel)
        {
            _globalScope = globalScope;
            _iniFile = iniFile;
            _windowManager = windowManager;
            _readModel = readModel;
        }

        public void StartFromRtu(Guid rtuId)
        {
            OriginalRtu = _readModel.Rtus.First(r => r.Id == rtuId);
            RtuId = OriginalRtu.Id.ToString();
            RtuName = OriginalRtu.Title;
            if (OriginalRtu.MainChannel.Ip4Address == "")
                OriginalRtu.MainChannel.Ip4Address = 
                    _iniFile.Read(IniSection.General, IniKey.Ip4Default, @"192.168.96.");
            MainChannelTestViewModel = _globalScope.Resolve<NetAddressTestViewModel>
                (new NamedParameter(@"netAddressForConnectionTest", new NetAddressForConnectionTest(OriginalRtu.MainChannel, true)));
            MainChannelTestViewModel.PropertyChanged += MainChannelTestViewModel_PropertyChanged;

            ReserveChannelTestViewModel = _globalScope.Resolve<NetAddressTestViewModel>
                (new NamedParameter(@"netAddressForConnectionTest", new NetAddressForConnectionTest(OriginalRtu.ReserveChannel, true)));
            ReserveChannelTestViewModel.PropertyChanged += ReserveChannelTestViewModel_PropertyChanged;

            IsReserveChannelEnabled = OriginalRtu.IsReserveChannelSet;

            OtdrAddressViewModel.FromRtu(OriginalRtu);
            if (OriginalRtu.RtuMaker == RtuMaker.IIT)
            {
                IitInfoModel.Model.FromRtu(OriginalRtu);
                IitVisibility = Visibility.Visible;
                VeexVisibility = Visibility.Collapsed;
            }
            else
            {
                VeexInfoModel.Model.FromRtu(OriginalRtu);
                IitVisibility = Visibility.Collapsed;
                VeexVisibility = Visibility.Visible;
            }
        }

        public void UpdateWithDto(RtuInitializedDto dto)
        {
            OriginalRtu.MainChannel = MainChannelTestViewModel.NetAddressInputViewModel.GetNetAddress();
            OriginalRtu.OtdrNetAddress = (NetAddress)dto.OtdrAddress.Clone();
            OriginalRtu.ReserveChannel = ReserveChannelTestViewModel.NetAddressInputViewModel.GetNetAddress();

            OtdrAddressViewModel.FromRtu(OriginalRtu);
            if (dto.Maker == RtuMaker.IIT)
            {
                IitInfoModel.Model.FromDto(dto);
                IitVisibility = Visibility.Visible;
                VeexVisibility = Visibility.Collapsed;
            }
            else
            {
                VeexInfoModel.Model.FromDto(dto);
                IitVisibility = Visibility.Collapsed;
                VeexVisibility = Visibility.Visible;
            }
        }

      
        private void ReserveChannelTestViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == @"Result")
            {
                if (ReserveChannelTestViewModel.Result == true)
                    _windowManager.ShowDialogWithAssignedOwner(
                        new MyMessageBoxViewModel(MessageType.Information, Resources.SID_RTU_connection_established_successfully_));
                if (ReserveChannelTestViewModel.Result == false)
                    _windowManager.ShowDialogWithAssignedOwner(
                        new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Cannot_establish_connection_with_RTU_));
            }
        }

        private void MainChannelTestViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == @"Result")
            {
                if (MainChannelTestViewModel.Result == true)
                    _windowManager.ShowDialogWithAssignedOwner(
                        new MyMessageBoxViewModel(MessageType.Information, Resources.SID_RTU_connection_established_successfully_));
                if (MainChannelTestViewModel.Result == false)
                    _windowManager.ShowDialogWithAssignedOwner(
                        new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Cannot_establish_connection_with_RTU_));
            }
        }

        #region Validate

        public bool Validate()
        {
            var initializedRtuCount = _readModel.Rtus.Count(r => r.OwnPortCount > 0);
            if (OriginalRtu.OwnPortCount > 0)
                initializedRtuCount--;
            if (_readModel.License.RtuCount.Value <= initializedRtuCount)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Exceeded_the_number_of_RTU_for_an_existing_license);
                _windowManager.ShowDialogWithAssignedOwner(vm);
                return false;
            }

            if (string.IsNullOrEmpty(OriginalRtu.Title))
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Title_should_be_set_);
                _windowManager.ShowDialogWithAssignedOwner(vm);
                return false;
            }

            if (!CheckAddressUniqueness())
                return false;
            return true;
        }

        private bool CheckAddressUniqueness()
        {
            var list = _readModel.Rtus.Where(r =>
                r.MainChannel.Ip4Address ==
                MainChannelTestViewModel.NetAddressInputViewModel.GetNetAddress().Ip4Address ||
                r.ReserveChannel.Ip4Address ==
                MainChannelTestViewModel.NetAddressInputViewModel.GetNetAddress().Ip4Address ||
                IsReserveChannelEnabled &&
                (r.MainChannel.Ip4Address ==
                 ReserveChannelTestViewModel.NetAddressInputViewModel.GetNetAddress().Ip4Address ||
                 r.ReserveChannel.Ip4Address ==
                 ReserveChannelTestViewModel.NetAddressInputViewModel.GetNetAddress().Ip4Address)).ToList();

            if (list.Count == 0 || list.Count == 1 && list.First().Id == OriginalRtu.Id)
                return true;

            var vm = new MyMessageBoxViewModel(MessageType.Error, Resources.SID_There_is_RTU_with_the_same_ip_address_);
            _windowManager.ShowDialogWithAssignedOwner(vm);
            return false;
        }
        #endregion
    }
}