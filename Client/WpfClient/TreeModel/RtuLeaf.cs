﻿using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class RtuLeaf : Leaf, IPortOwner
    {
        public readonly ILifetimeScope GlobalScope;
        private readonly IMyLog _logFile;

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

        private RtuPartState _bopState;
        public RtuPartState BopState
        {
            get { return _bopState; }
            set
            {
                if (value == _bopState) return;
                _bopState = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(BopPictogram));
            }
        }

        private RtuPartState _mainChannelState;
        public RtuPartState MainChannelState
        {
            get { return _mainChannelState; }
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
            get { return _reserveChannelState; }
            set
            {
                if (value == _reserveChannelState) return;
                _reserveChannelState = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(ReserveChannelPictogram));
            }
        }

        public bool IsAvailable => MainChannelState == RtuPartState.Normal ||
                                   ReserveChannelState == RtuPartState.Normal;

        public ImageSource MonitoringPictogram => MonitoringState.GetPictogram();
        public ImageSource BopPictogram => BopState.GetPictogram();
        public ImageSource MainChannelPictogram => MainChannelState.GetPictogram();
        public ImageSource ReserveChannelPictogram => ReserveChannelState.GetPictogram();
        #endregion

        public int OwnPortCount { get; set; }
        public int FullPortCount { get; set; }
        public string Serial { get; set; }
        public NetAddress OtauNetAddress { get; set; }
        public override string Name => Title;

        public ChildrenImpresario ChildrenImpresario { get; }
        public bool HasAttachedTraces => ChildrenImpresario.Children.Any(l => l is TraceLeaf && ((TraceLeaf)l).PortNumber > 0);
        public int TraceCount => ChildrenImpresario.Children.Count(c => c is TraceLeaf) +
                ChildrenImpresario.Children.Where(c => c is OtauLeaf).Sum(otauLeaf => ((OtauLeaf)otauLeaf).TraceCount);

        public IPortOwner GetOwnerOfExtendedPort(int extendedPortNumber)
        {
            if (extendedPortNumber <= OwnPortCount)
                return this;
            foreach (var child in ChildrenImpresario.Children)
            {
                var otau = child as OtauLeaf;
                if (otau != null &&
                    extendedPortNumber >= otau.FirstPortNumber &&
                    extendedPortNumber < otau.FirstPortNumber + otau.OwnPortCount)
                    return otau;
            }
            return null;
        }

        public RtuLeaf(ILifetimeScope globalScope, IMyLog logFile, ReadModel readModel, IWindowManager windowManager, IWcfServiceForClient c2DWcfManager,
            PostOffice postOffice, FreePorts view)
            : base(readModel, windowManager, c2DWcfManager, postOffice)
        {
            GlobalScope = globalScope;
            _logFile = logFile;
            ChildrenImpresario = new ChildrenImpresario(view);

            Title = Resources.SID_noname_RTU;
            Color = Brushes.DarkGray;
            IsExpanded = true;
        }
        protected override List<MenuItemVm> GetMenuItems()
        {
            return new RtuLeafContextMenuProvider(_logFile).GetMenu(this);
        }




    }
}