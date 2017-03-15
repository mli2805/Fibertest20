using Iit.Fibertest.Graph;
using Iit.Fibertest.TestBench;

namespace Graph.Tests
{
    public class SutForEquipment : SystemUnderTest
    {
        public const string NewTitleForTest = "New name for old equipment";
        public const EquipmentType NewTypeForTest = EquipmentType.Cross;
        public const int NewLeftCableReserve = 15;
        public const int NewRightCableReserve = 7;
        public const string NewCommentForTest = "New comment for old equipment";

        public bool EquipmentInfoViewModelHandler(object model, Answer button)
        {
            var vm = model as EquipmentInfoViewModel;
            if (vm == null) return false;

            vm.Title = NewTitleForTest;
            vm.Type = NewTypeForTest;
            vm.CableReserveLeft = NewLeftCableReserve;
            vm.CableReserveRight = NewRightCableReserve;
            vm.Comment = NewCommentForTest;

            if (button == Answer.Yes)
                vm.Save();
            else
                vm.Cancel();
            return true;
        }
    }
}