﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class RtuLeaf : Leaf, IPortOwner
    {
        private readonly RtuLeafContextMenuProvider _rtuLeafContextMenuProvider;
        

        #region Pictograms
        private MonitoringState _monitoringState;
        public MonitoringState MonitoringState
        {
            get { return _monitoringState; }
            set
            {
                if (value == _monitoringState) return;
                _monitoringState = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(MonitoringPictogram));
            }
        }

        // pair OTAU ID - is OK or not
        private Dictionary<Guid, bool> _otauStates = new Dictionary<Guid, bool>();
        public void SetOtauState(Guid otauId, bool isOkOrNot)
        {
            if (_otauStates.ContainsKey(otauId))
                _otauStates[otauId] = isOkOrNot;
            else
                _otauStates.Add(otauId, isOkOrNot);

            NotifyOfPropertyChange(nameof(BopPictogram));
        }

        public void RemoveOtauState(Guid otauId)
        {
            if (_otauStates.ContainsKey(otauId))
                _otauStates.Remove(otauId);

            NotifyOfPropertyChange(nameof(BopPictogram));
        }

        public RtuPartState BopState
        {
            get
            {
                return _otauStates.Count == 0
                    ? RtuPartState.NotSetYet
                    : _otauStates.Any(s => s.Value != true)
                        ? RtuPartState.Broken
                        : RtuPartState.Ok;
            }
        }

        private RtuPartState _mainChannelState;
        public RtuPartState MainChannelState
        {
            get => _mainChannelState;
            set
            {
                if (value == _mainChannelState) return;
                _mainChannelState = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(MainChannelPictogram));
            }
        }

        private RtuPartState _reserveChannelState;
        public RtuPartState ReserveChannelState
        {
            get => _reserveChannelState;
            set
            {
                if (value == _reserveChannelState) return;
                _reserveChannelState = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(ReserveChannelPictogram));
            }
        }

        public bool IsAvailable => MainChannelState == RtuPartState.Ok ||
                                   ReserveChannelState == RtuPartState.Ok;

        public Uri MonitoringPictogram => MonitoringState.GetPictogramUri();
        public string BopPictogram => BopState.GetPathToPictogram();
        public string MainChannelPictogram => MainChannelState.GetPathToPictogram();
        public string ReserveChannelPictogram => ReserveChannelState.GetPathToPictogram();
        #endregion

        public RtuMaker RtuMaker { get; set; }
        public int OwnPortCount { get; set; }
        public int FullPortCount { get; set; }
        public string Serial { get; set; }
        public NetAddress OtauNetAddress { get; set; }
        public override string Name => Title;
        public TreeOfAcceptableMeasParams TreeOfAcceptableMeasParams { get; set; }

        public ChildrenImpresario ChildrenImpresario { get; }
        public bool HasAttachedTraces => ChildrenImpresario.Children.Any(l => l is TraceLeaf && ((TraceLeaf)l).PortNumber > 0)
                   || ChildrenImpresario.Children.Any(c => c is OtauLeaf && ((OtauLeaf)c).HasAttachedTraces);

        public int TraceCount => ChildrenImpresario.Children.Count(c => c is TraceLeaf) +
                ChildrenImpresario.Children.Where(c => c is OtauLeaf).Sum(otauLeaf => ((OtauLeaf)otauLeaf).TraceCount);

        public IPortOwner GetPortOwner(string serial)
        {
            if (Serial == serial) return this;
            return ChildrenImpresario.Children.Select(child => child as OtauLeaf).
                FirstOrDefault(otau => otau?.Serial == serial);
        }

        public RtuLeaf(RtuLeafContextMenuProvider rtuLeafContextMenuProvider, FreePorts view)
        {
            _rtuLeafContextMenuProvider = rtuLeafContextMenuProvider;
            ChildrenImpresario = new ChildrenImpresario(view);

            Title = Resources.SID_noname_RTU;
            Color = Brushes.DarkGray;
            MonitoringState = MonitoringState.Unknown;
            IsExpanded = true;
        }
        protected override List<MenuItemVm> GetMenuItems()
        {
            return _rtuLeafContextMenuProvider.GetMenu(this);
        }

    }
}