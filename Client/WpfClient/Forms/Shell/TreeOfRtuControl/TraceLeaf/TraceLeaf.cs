﻿using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class TraceLeaf : Leaf, IPortNumber
    {
        private int _portNumber;
        public int PortNumber
        {
            get => _portNumber;
            set
            {
                if (value == _portNumber) return;
                _portNumber = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(IconsVisibility));
                NotifyOfPropertyChange(nameof(LeftMargin));
                NotifyOfPropertyChange(nameof(Name));
            }
        }

        public int LeftMargin => PortNumber < 1 ? 78 : Parent is RtuLeaf ? 53 : 74;
        public Visibility IconsVisibility => PortNumber > 0 ? Visibility.Visible : Visibility.Hidden;

        public override string Name
        {
            get
            {
                return PortNumber < 1
                  ? Title
                  : string.Format(Resources.SID_Port_trace, PortNumber, Title);
            }
            set { }
        }

        private BaseRefsSet _baseRefsSet = new BaseRefsSet();
        public BaseRefsSet BaseRefsSet
        {
            get { return _baseRefsSet; }
            set
            {
                if (Equals(value, _baseRefsSet)) return;
                _baseRefsSet = value;
                NotifyOfPropertyChange();
            }
        }

     


        private FiberState _traceState;
        public FiberState TraceState
        {
            get => _traceState;
            set
            {
                if (value == _traceState) return;
                _traceState = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(TraceStatePictogram));
            }
        }

        public ImageSource TraceStatePictogram => TraceState.GetPictogram();

        private readonly TraceLeafContextMenuProvider _contextMenuProvider;

        public TraceLeaf(IPortOwner parent, TraceLeafContextMenuProvider contextMenuProvider)
        {
            Parent = (Leaf)parent;
            _contextMenuProvider = contextMenuProvider;
        }

        protected override List<MenuItemVm> GetMenuItems()
        {
            return _contextMenuProvider.GetMenu(this);
        }

     
    }
}



