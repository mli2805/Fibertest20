using System;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Client;
using Iit.Fibertest.Dto;
using Iit.Fibertest.WpfCommonViews;


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

        public static bool WaitFormHandler(this SystemUnderTest sut, object model)
        {
            if (!(model is WaitViewModel)) return false;
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
                if (selectedOptionNumber < vm.EquipmentChoices.Count)
                    vm.EquipmentChoices[selectedOptionNumber].IsSelected = true;
                else
                    vm.NoEquipmentInNodeChoice.IsSelected = true;
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
            vm.Title = title;
            vm.Model.Comment = comment;
            if (button == Answer.Yes)
                vm.Save();
            else
                vm.Cancel();
            return true;
        }

        public static bool TraceChoiceHandler(this SystemUnderTest sut, object model, List<Guid> chosenTraces, Answer answer)
        {
            if (!(model is TracesToEquipmentInjectionViewModel vm)) return false;
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
            vm.Count = count.ToString();
            vm.SetSelectedType(type);
            if (answer == Answer.Yes)
                vm.Save();
            else
                vm.Cancel();
            return true;
        }

        public static bool BopStateHandler(this SystemUnderTest sut, object model)
        {
            if (!(model is BopStateViewModel)) return false;
            return true;
        }

        public static bool FiberUpdateHandler(this SystemUnderTest sut, object model, int userInputedLength, Answer answer)
        {
            if (!(model is FiberUpdateViewModel vm)) return false;
            vm.UserInputedLength = userInputedLength.ToString();
            if (answer == Answer.Yes)
                vm.Save();
            else
                vm.Cancel();
            return true;
        }

        public static bool NoLicenseHandler(this SystemUnderTest sut, object model, string filename = "")
        {
            if (!(model is NoLicenseAppliedViewModel vm)) return false;
            if (filename == "")
                vm.ApplyDemoLicense();
            else
                vm.LoadLicenseFromFile(filename);
            return true;
        }

        public static bool SecurityAdminPasswordHandler(this SystemUnderTest sut, object model, string password)
        {
            if (!(model is SecurityAdminConfirmationViewModel vm)) return false;
            vm.Initialize();
            vm.Password = password;
            vm.OkButton();
            return true;
        }

        public static bool ZoneHandler(this SystemUnderTest sut, object model, string zoneTitle, Answer answer)
        {
            if (!(model is ZoneViewModel vm)) return false;
            vm.Initialize();
            vm.Title = zoneTitle;
            if (answer == Answer.Yes)
                vm.Save();
            else
                vm.Cancel();
            return true;
        }

        public static bool UserHandler(this SystemUnderTest sut, object model, string username, string password, Guid zoneId, Answer answer)
        {
            if (!(model is UserViewModel vm)) return false;
            vm.InitializeForCreate();
            vm.UserInWork.Title = username;
            vm.Password1 = password;
            vm.Password2 = password;
            vm.UserInWork.Role = Role.Operator;
            vm.SelectedZone = vm.Zones.First(z => z.ZoneId == zoneId);
            if (answer == Answer.Yes)
                vm.Save();
            else
                vm.Cancel();
            return true;
        }


        public static bool EquipmentInfoViewModelHandler(this SystemUnderTest sut, object model, Answer button,
            EquipmentType equipmentType = SystemUnderTest.NewTypeForTest,
            int cableReserveLeft = SystemUnderTest.NewLeftCableReserve,
            int cableReserveRight = SystemUnderTest.NewRightCableReserve,
            string newEquipmentTitle = "")
        {
            if (!(model is EquipmentInfoViewModel vm)) return false;

            vm.Model.Title = newEquipmentTitle == "" ? SystemUnderTest.NewTitleForTest : newEquipmentTitle;
            vm.Model.SetSelectedRadioButton(equipmentType);
            vm.Model.CableReserveLeft = cableReserveLeft;
            vm.Model.CableReserveRight = cableReserveRight;
            vm.Model.Comment = SystemUnderTest.NewCommentForTest;

            if (button == Answer.Yes)
                vm.Save();
            else
                vm.Cancel();
            return true;
        }
    }
}