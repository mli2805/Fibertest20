using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.TestBench;
using Serilog;

namespace Graph.Tests
{
    public class SystemUnderTest
    {
        public ReadModel ReadModel { get; }
        public ILogger LoggerForTests { get; set; }
        public ClientPoller Poller { get; }
        public FakeWindowManager FakeWindowManager { get; }
        public ShellViewModel ShellVm { get; }
        public int CurrentEventNumber => Poller.CurrentEventNumber;
        public const string Path = @"..\..\Sut\base.sor";

        public SystemUnderTest()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<AutofacEventSourcing>();
            builder.RegisterModule<AutofacUi>();
            builder.RegisterType<FakeWindowManager>().As<IWindowManager>().SingleInstance();

            var container = builder.Build();
            Poller = container.Resolve<ClientPoller>();
            FakeWindowManager =(FakeWindowManager) container.Resolve<IWindowManager>();
            ReadModel = container.Resolve<ReadModel>();

            ShellVm = (ShellViewModel) container.Resolve<IShell>();

            LoggerForTests = new LoggerConfiguration()
                    .WriteTo.Console()
                    .CreateLogger();
        }

        public Iit.Fibertest.Graph.Trace CreateTraceRtuEmptyTerminal()
        {
            ShellVm.ComplyWithRequest(new RequestAddRtuAtGpsLocation() {Latitude = 66, Longitude = 30}).Wait();
            Poller.Tick();
            var nodeForRtuId = ReadModel.Rtus.Last().NodeId;

            ShellVm.ComplyWithRequest(new AddNode()).Wait();
            Poller.Tick();
            var firstNodeId = ReadModel.Nodes.Last().Id;

            ShellVm.ComplyWithRequest(new RequestAddEquipmentAtGpsLocation() {Type = EquipmentType.Terminal}).Wait();
            Poller.Tick();
            var secondNodeId = ReadModel.Nodes.Last().Id;

            ShellVm.ComplyWithRequest(new AddFiber() {Node1 = nodeForRtuId, Node2 = firstNodeId}).Wait();
            ShellVm.ComplyWithRequest(new AddFiber() {Node1 = firstNodeId, Node2 = secondNodeId }).Wait();
            Poller.Tick();

            return DefineTrace(secondNodeId, nodeForRtuId);
        }


        protected Iit.Fibertest.Graph.Trace DefineTrace(Guid lastNodeId, Guid nodeForRtuId)
        {
            FakeWindowManager.RegisterHandler(model => QuestionAnswer(Resources.SID_Accept_the_path, Answer.Yes, model));
            FakeWindowManager.RegisterHandler(model => EquipmentChoiceHandler(model, EquipmentChoiceAnswer.Continue, 0));
            FakeWindowManager.RegisterHandler(model => AddTraceViewHandler(model, @"some title", "", Answer.Yes));
            ShellVm.ComplyWithRequest(new RequestAddTrace() {LastNodeId = lastNodeId, NodeWithRtuId = nodeForRtuId});
            Poller.Tick();
            return ShellVm.ReadModel.Traces.Last();
        }


        public bool QuestionAnswer(string question, Answer answer, object model)
        {
            var vm = model as QuestionViewModel;
            if (vm == null) return false;
            if (vm.QuestionMessage != question) return false;
            if (answer == Answer.Yes)
                vm.OkButton();
            else
                vm.CancelButton();
            return true;
        }

        public bool NodeUpdateHandler(object model, string title, string comment, Answer button)
        {
            var vm = model as NodeUpdateViewModel;
            if (vm == null) return false;
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

        public bool RtuUpdateHandler(object model, string title, string comment, Answer button)
        {
            var vm = model as RtuUpdateViewModel;
            if (vm == null) return false;
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

        public bool EquipmentChoiceHandler(object model, EquipmentChoiceAnswer answer, int chosenEquipmentNumber)
        {
            var vm = model as EquipmentChoiceViewModel;
            if (vm == null) return false;
            switch (answer)
            {
                case EquipmentChoiceAnswer.Continue:
                    vm.Choices[chosenEquipmentNumber].IsChecked = true;
                    vm.SelectButton();
                    break;
                case EquipmentChoiceAnswer.SetupNameAndContinue:
                    vm.Choices[chosenEquipmentNumber].IsChecked = true;
                    vm.SelectAndSetupNameButton();
                    break;
                case EquipmentChoiceAnswer.Cancel:
                    vm.CancelButton();
                    break;
            }
            return true;
        }

        public bool AddTraceViewHandler(object model, string title, string comment, Answer button)
        {
            var vm = model as TraceInfoViewModel;
            if (vm == null) return false;
            vm.IsInTraceCreationMode = true;
            vm.Title = title;
            vm.Comment = comment;
            if (button == Answer.Yes)
                vm.Save();
            else
                vm.Cancel();
            return true;
        }

        public bool TraceChoiceHandler(object model, List<Guid> chosenTraces, Answer answer)
        {
            var vm = model as TraceChoiceViewModel;
            if (vm == null) return false;
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

        public bool FiberWithNodesAdditionHandler(object model, int count, EquipmentType type, Answer answer)
        {
            var vm = model as FiberWithNodesAddViewModel;
            if (vm == null) return false;
            vm.Count = count;
            vm.SetSelectedType(type);
            if (answer == Answer.Yes)
                vm.Save();
            else
                vm.Cancel();
            return true;
        }

        public bool FiberUpdateHandler(object model, Answer answer)
        {
            var vm = model as FiberUpdateViewModel;
            if (vm == null) return false;
            if (answer == Answer.Yes)
                vm.Save();
            else
                vm.Cancel();
            return true;
        }
    }
}