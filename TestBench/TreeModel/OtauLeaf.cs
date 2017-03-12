using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.TestBench
{
    public class OtauLeaf : Leaf, IPortOwner
    {
        public RtuPartState State { get; set; }
        public int PortCount { get; set; }
        public int FirstPortNumber { get; set; }
        public int MasterPort { get; set; }
        public RtuPartState OtauState { get; set; }
        public ImageSource OtauStatePictogram => OtauState.GetPictogram();

        public override string Name
        {
            get { return string.Format(Resources.SID_Port_trace, MasterPort, Title); }
            set { }
        }
        public ObservableCollection<Leaf> Children { get; set; } = new ObservableCollection<Leaf>();

        public OtauLeaf(ReadModel readModel, IWindowManager windowManager,
            Bus bus ,FreePortsToggleButton freePortsToggleButton) : base(readModel, windowManager, bus)
        {
            ChildrenProvider = new ChildrenProvider(Children, freePortsToggleButton);
        }
        protected override List<MenuItemVm> GetMenuItems()
        {
            IsSelected = true;

            return new List<MenuItemVm>
            {
                new MenuItemVm()
                {
                    Header = Resources.SID_Remove,
                    Command = new ContextMenuAction(OtauRemoveAction, CanOtauRemoveAction),
                    CommandParameter = this
                }
            };
        }

        public void OtauRemoveAction(object param)
        {
            Bus.SendCommand(new DetachOtau() { Id = Id, RtuId = Parent.Id });
        }

        private bool CanOtauRemoveAction(object param)
        {
            return Children.All(c => c is PortLeaf);
        }

        public ChildrenProvider ChildrenProvider { get; }
    }
}