﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class TcesViewModel : Screen
    {
        private readonly ILifetimeScope _globalScope;
        private readonly Model _readModel;
        private readonly IWcfServiceDesktopC2D _c2DWcfManager;
        private readonly IWindowManager _windowManager;
        private readonly CurrentUser _currentUser;
        private ObservableCollection<TceS> _tces;

        public ObservableCollection<TceS> Tces
        {
            get => _tces;
            set
            {
                if (Equals(value, _tces)) return;
                _tces = value;
                NotifyOfPropertyChange();
            }
        }

        public TceS SelectedTce { get; set; }
        public bool IsEnabled { get; set; }

        public TcesViewModel(ILifetimeScope globalScope, Model readModel, EventArrivalNotifier eventArrivalNotifier,
            IWcfServiceDesktopC2D c2DWcfManager, IWindowManager windowManager, CurrentUser currentUser)
        {
            _globalScope = globalScope;
            _readModel = readModel;
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;
            _currentUser = currentUser;
            eventArrivalNotifier.PropertyChanged += _eventArrivalNotifier_PropertyChanged;
        }

        private void _eventArrivalNotifier_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Tces = new ObservableCollection<TceS>(_readModel.TcesNew);
        }

        public void Initialize()
        {
            IsEnabled = _currentUser.Role <= Role.Root;
            Tces = new ObservableCollection<TceS>(_readModel.TcesNew);
            if (Tces.Count > 0)
                SelectedTce = Tces.First();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Telecommunications_equipment;
        }

        public void AddTce()
        {
            var vm = _globalScope.Resolve<TceTypeViewModel>();
            vm.Initialize(_readModel.TceTypeStructs.First(), true);
            if (_windowManager.ShowDialogWithAssignedOwner(vm) != true)
                return;

            var selectedTceType = vm.SelectedTabItem == 0
                ? vm.HuaweiSelectionViewModel.SelectedType
                : vm.ZteSelectionViewModel.SelectedType;

            var ovm = _globalScope.Resolve<OneTceViewModel>();
            var tce = selectedTceType.CreateTce();
            ovm.Initialize(tce);
            _windowManager.ShowWindowWithAssignedOwner(ovm);
        }

        public async void ChangeTceType()
        {
            var typeId = SelectedTce.TceTypeStruct.Id;
            if (!AskNewTceTypeSelection(ref typeId))
                return;

            var tracesLostRelations = 
                AdjustSelectedTceToNewType(_readModel.TceTypeStructs.First(t=>t.Id == typeId))
                                                                    .Select(g => g.TraceId);

            var cmd = new AddOrUpdateTceWithRelations()
            {
                Id = SelectedTce.Id,
                Title = SelectedTce.Title,
                TceTypeStruct = SelectedTce.TceTypeStruct,
                Ip = SelectedTce.Ip,
                Slots = SelectedTce.Slots,
                Comment = SelectedTce.Comment,
                AllRelationsOfTce = _readModel.GponPortRelations
                    .Where(r=>r.TceId == SelectedTce.Id 
                              && !tracesLostRelations.Contains(r.TraceId))
                    .ToList(),
            };

            var result = await _c2DWcfManager.SendCommandAsObj(cmd);
            if (result != null)
            {
                _windowManager.ShowDialogWithAssignedOwner(new MyMessageBoxViewModel(MessageType.Error, result));
            }
            else
            {
                _windowManager.ShowDialogWithAssignedOwner(new MyMessageBoxViewModel(MessageType.Information,
                    Resources.SID_Equipment_type_changed_successfully_));
            }
        }

        private bool AskNewTceTypeSelection(ref int newTceTypeId)
        {
            var oldTypeId = newTceTypeId;
            var vm = _globalScope.Resolve<TceTypeViewModel>();
            vm.Initialize(SelectedTce.TceTypeStruct, false);
            if (_windowManager.ShowDialogWithAssignedOwner(vm) != true)
                return false;

            newTceTypeId = vm.SelectedTabItem == 0
                ? vm.HuaweiSelectionViewModel.SelectedType.Id
                : vm.ZteSelectionViewModel.SelectedType.Id;
            return oldTypeId != newTceTypeId;
        }

        private List<GponPortRelation> AdjustSelectedTceToNewType(TceTypeStruct newTceType)
        {
            var temp = new TceS(SelectedTce);

            SelectedTce.TceTypeStruct = newTceType;
            var relationsForRemoval = new List<GponPortRelation>();
            foreach (var slot in temp.Slots)
            {
                if (SelectedTce.TceTypeStruct.SlotPositions.Contains(slot.Position)) continue;

                relationsForRemoval.AddRange(_readModel.GponPortRelations
                    .Where(r => r.TceId == SelectedTce.Id && r.SlotPosition == slot.Position));

                // foreach (var relation in _readModel.GponPortRelations.Where(r => r.TceId == SelectedTce.Id && r.SlotPosition == slot.Position).ToList())
                // {
                //     _readModel.GponPortRelations.Remove(relation);
                // }
            }

            SelectedTce.Slots = new List<TceSlot>();
            foreach (var slotPosition in SelectedTce.TceTypeStruct.SlotPositions)
            {
                var oldSlot = temp.Slots.FirstOrDefault(s => s.Position == slotPosition);
                SelectedTce.Slots.Add(oldSlot ?? new TceSlot() { Position = slotPosition });
            }

            var diff = temp.TceTypeStruct.GponInterfaceNumerationFrom -
                       SelectedTce.TceTypeStruct.GponInterfaceNumerationFrom;
            if (diff != 0)
                foreach (var relation in _readModel.GponPortRelations.Where(r => r.TraceId == SelectedTce.Id))
                    relation.GponInterface -= diff;

            return relationsForRemoval;
        }

        // button Settings
        public void UpdateTceComponents()
        {
            if (SelectedTce == null) return;
            var vm = _globalScope.Resolve<OneTceViewModel>();
            vm.Initialize(SelectedTce);
            _windowManager.ShowWindowWithAssignedOwner(vm);
        }

        public async void RemoveTce()
        {
            if (SelectedTce == null) return;
            if (!ConfirmTceRemove()) return;

            var cmd = new RemoveTce() { Id = SelectedTce.Id };
            if (await _c2DWcfManager.SendCommandAsObj(cmd) == null)
            {
                Tces.Remove(SelectedTce);
            }
        }

        private bool ConfirmTceRemove()
        {
            var strs = new List<string>()
            {
                string.Format(Resources.SID_Equipment__0__will_be_deleted, SelectedTce.Title),
                "",
                Resources.SID_RTU_port_links_to_this_equipment_will_be_deleted
            };
            var vm = new MyMessageBoxViewModel(MessageType.Confirmation, strs, 0);
            _windowManager.ShowDialogWithAssignedOwner(vm);
            return vm.IsAnswerPositive;
        }

        public void Close()
        {
            TryClose();
        }

    }
}
