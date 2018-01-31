using System;
using System.Collections.Generic;
using Iit.Fibertest.Client;
using Iit.Fibertest.Graph;

namespace Graph.Tests
{
    public static class Handlers
    {
        public static bool OneLineMessageBoxAnswer(this SystemUnderTest sut, string question, Answer answer, object model)
        {
            if (!(model is MyMessageBoxViewModel vm)) return false;
            if (vm.Lines[0].Line != question) return false;
            if (answer == Answer.Yes)
                vm.OkButton();
            else
                vm.CancelButton();
            return true;
        }

        public static bool ManyLinesMessageBoxAnswer(this SystemUnderTest sut, Answer answer, object model)
        {
            if (!(model is MyMessageBoxViewModel vm)) return false;
            if (answer == Answer.Yes)
                vm.OkButton();
            else
                vm.CancelButton();
            return true;
        }

        public static bool NodeUpdateHandler(this SystemUnderTest sut, object model, string title, string comment, Answer button)
        {
            if (!(model is NodeUpdateViewModel vm)) return false;
            if (title != null)
                vm.Title = title;
            if (comment != null)
                vm.Comment = comment;
            if (button == Answer.Yes)
                vm.Save();
            else
                vm.Cancel();
            return true;
        }

        public static bool RtuUpdateHandler(this SystemUnderTest sut, object model, string title, string comment, Answer button)
        {
            if (!(model is RtuUpdateViewModel vm)) return false;
            if (title != null)
                vm.Title = title;
            if (comment != null)
                vm.Comment = comment;
            if (button == Answer.Yes)
                vm.Save();
            else
                vm.Cancel();
            return true;
        }

        public static bool TraceContentChoiceHandler(this SystemUnderTest sut, object model, Answer button, int selectedOptionNumber)
        {
            if (!(model is TraceContentChoiceViewModel vm)) return false;
            if (button == Answer.Yes)
            {
                vm.Choices[selectedOptionNumber].IsSelected = true;
                vm.NextButton();
            }
            else
            {
                vm.CancelButton();
            }

            return true;
        }

        public static bool AddTraceViewHandler(this SystemUnderTest sut, object model, string title, string comment, Answer button)
        {
            if (!(model is TraceInfoViewModel vm)) return false;
            vm.Model.Title = title;
            vm.Model.Comment = comment;
            if (button == Answer.Yes)
                vm.Save();
            else
                vm.Cancel();
            return true;
        }

        public static bool TraceChoiceHandler(this SystemUnderTest sut, object model, List<Guid> chosenTraces, Answer answer)
        {
            if (!(model is TraceChoiceViewModel vm)) return false;
            foreach (var chosenTrace in chosenTraces)
            {
                foreach (var checkbox in vm.Choices)
                {
                    if (checkbox.Id == chosenTrace)
                        checkbox.IsChecked = true;
                }
            }

            if (answer == Answer.Yes)
                vm.Accept();
            else
                vm.Cancel();
            return true;
        }

        public static bool FiberWithNodesAdditionHandler(this SystemUnderTest sut, object model, int count, EquipmentType type, Answer answer)
        {
            if (!(model is FiberWithNodesAddViewModel vm)) return false;
            vm.Count = count;
            vm.SetSelectedType(type);
            if (answer == Answer.Yes)
                vm.Save();
            else
                vm.Cancel();
            return true;
        }
        public static bool FiberUpdateHandler(this SystemUnderTest sut, object model, Answer answer)
        {
            if (!(model is FiberUpdateViewModel vm)) return false;
            if (answer == Answer.Yes)
                vm.Save();
            else
                vm.Cancel();
            return true;
        }

        public static bool EquipmentInfoViewModelHandler(this SystemUnderTest sut, object model, Answer button)
        {
            if (!(model is EquipmentInfoViewModel vm)) return false;

            vm.Model.Title = SystemUnderTest.NewTitleForTest;
            vm.Model.SetSelectedRadioButton(SystemUnderTest.NewTypeForTest);
            vm.Model.CableReserveLeft = SystemUnderTest.NewLeftCableReserve;
            vm.Model.CableReserveRight = SystemUnderTest.NewRightCableReserve;
            vm.Model.Comment = SystemUnderTest.NewCommentForTest;

            if (button == Answer.Yes)
                vm.Save();
            else
                vm.Cancel();
            return true;
        }
    }
}