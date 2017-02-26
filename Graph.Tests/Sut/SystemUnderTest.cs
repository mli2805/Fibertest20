using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.TestBench;

namespace Graph.Tests
{
    public class SystemUnderTest
    {
        public ReadModel ReadModel { get; }
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
        }

        public Iit.Fibertest.Graph.Trace CreateTraceRtuEmptyTerminal()
        {
            ShellVm.ComplyWithRequest(new AddRtuAtGpsLocation() {Latitude = 55, Longitude = 30}).Wait();
            Poller.Tick();
            var nodeForRtuId = ReadModel.Rtus.Last().NodeId;

            ShellVm.ComplyWithRequest(new AddNode()).Wait();
            Poller.Tick();
            var firstNodeId = ReadModel.Nodes.Last().Id;

            ShellVm.ComplyWithRequest(new AddEquipmentAtGpsLocation() {Type = EquipmentType.Terminal}).Wait();
            Poller.Tick();
            var secondNodeId = ReadModel.Nodes.Last().Id;

            ShellVm.ComplyWithRequest(new AddFiber() {Node1 = nodeForRtuId, Node2 = firstNodeId}).Wait();
            ShellVm.ComplyWithRequest(new AddFiber() {Node1 = firstNodeId, Node2 = secondNodeId }).Wait();
            Poller.Tick();

            FakeWindowManager.RegisterHandler(model => QuestionAnswer(Resources.SID_Accept_the_path, Answer.Yes, model));
            FakeWindowManager.RegisterHandler(model => EquipmentChoiceHandler(model, EquipmentChoiceAnswer.Continue, 0));
            FakeWindowManager.RegisterHandler(model => AddTraceViewHandler(model, @"some trace title", "", Answer.Yes));

            ShellVm.ComplyWithRequest(new RequestAddTrace() { LastNodeId = secondNodeId, NodeWithRtuId = nodeForRtuId }).Wait();
            Poller.Tick();
            return ReadModel.Traces.Last();
        }

        public void CreatePositionForAddNodeIntoFiberTest(out Iit.Fibertest.Graph.Fiber fiberForInsertion, out Iit.Fibertest.Graph.Trace traceForInsertionId)
        {
            ShellVm.ComplyWithRequest(new AddRtuAtGpsLocation() {Latitude = 55, Longitude = 30}).Wait();
            Poller.Tick();
            var nodeForRtuId = ReadModel.Rtus.Last().NodeId;
            ShellVm.ComplyWithRequest(new AddNode()).Wait();
            Poller.Tick();
            var a1 = ReadModel.Nodes.Last().Id;
            ShellVm.ComplyWithRequest(new AddNode()).Wait();
            Poller.Tick();
            var b1 = ReadModel.Nodes.Last().Id;
            // fiber for insertion
            ShellVm.ComplyWithRequest(new AddFiber() {Node1 = a1, Node2 = b1 }).Wait();
            Poller.Tick();
            fiberForInsertion = ShellVm.ReadModel.Fibers.Last();

            ShellVm.ComplyWithRequest(new AddEquipmentAtGpsLocation() {Type = EquipmentType.Terminal}).Wait();
            Poller.Tick();
            var a2 = ReadModel.Nodes.Last().Id;
            ShellVm.ComplyWithRequest(new AddEquipmentAtGpsLocation() {Type = EquipmentType.Terminal}).Wait();
            Poller.Tick();
            var b2 = ReadModel.Nodes.Last().Id;
            ShellVm.ComplyWithRequest(new AddEquipmentAtGpsLocation() {Type = EquipmentType.Terminal}).Wait();
            Poller.Tick();
            var c2 = ReadModel.Nodes.Last().Id;
            ShellVm.ComplyWithRequest(new AddEquipmentAtGpsLocation() {Type = EquipmentType.Terminal}).Wait();
            Poller.Tick();
            var d2 = ReadModel.Nodes.Last().Id;

            ShellVm.ComplyWithRequest(new AddFiber() {Node1 = nodeForRtuId, Node2 = a1 }).Wait();
            ShellVm.ComplyWithRequest(new AddFiber() {Node1 = a1, Node2 = a2 }).Wait();
            ShellVm.ComplyWithRequest(new AddFiber() {Node1 = b1, Node2 = b2 }).Wait();
            ShellVm.ComplyWithRequest(new AddFiber() {Node1 = b1, Node2 = c2 }).Wait();
            ShellVm.ComplyWithRequest(new AddFiber() {Node1 = nodeForRtuId, Node2 = d2 }).Wait();
            ShellVm.ComplyWithRequest(new AddFiber() {Node1 = b1, Node2 = a2 }).Wait();
            Poller.Tick();

            FakeWindowManager.RegisterHandler(model => QuestionAnswer(Resources.SID_Accept_the_path, Answer.Yes, model));
            FakeWindowManager.RegisterHandler(model => EquipmentChoiceHandler(model, EquipmentChoiceAnswer.Continue, 0));
            FakeWindowManager.RegisterHandler(model => AddTraceViewHandler(model, @"some title", "", Answer.Yes));
            ShellVm.ComplyWithRequest(new RequestAddTrace() { LastNodeId = a2, NodeWithRtuId = nodeForRtuId }).Wait();
            Poller.Tick();
            traceForInsertionId = ShellVm.ReadModel.Traces.Last();

            FakeWindowManager.RegisterHandler(model => QuestionAnswer(Resources.SID_Accept_the_path, Answer.Yes, model));
            FakeWindowManager.RegisterHandler(model => EquipmentChoiceHandler(model, EquipmentChoiceAnswer.Continue, 0));
            FakeWindowManager.RegisterHandler(model => AddTraceViewHandler(model, @"some title", "", Answer.Yes));
            ShellVm.ComplyWithRequest(new RequestAddTrace() { LastNodeId = b2, NodeWithRtuId = nodeForRtuId }).Wait();
            Poller.Tick();

            FakeWindowManager.RegisterHandler(model => QuestionAnswer(Resources.SID_Accept_the_path, Answer.Yes, model));
            FakeWindowManager.RegisterHandler(model => EquipmentChoiceHandler(model, EquipmentChoiceAnswer.Continue, 0));
            FakeWindowManager.RegisterHandler(model => AddTraceViewHandler(model, @"some title", "", Answer.Yes));
            ShellVm.ComplyWithRequest(new RequestAddTrace() { LastNodeId = c2, NodeWithRtuId = nodeForRtuId }).Wait();
            Poller.Tick();

            FakeWindowManager.RegisterHandler(model => QuestionAnswer(Resources.SID_Accept_the_path, Answer.Yes, model));
            FakeWindowManager.RegisterHandler(model => EquipmentChoiceHandler(model, EquipmentChoiceAnswer.Continue, 0));
            FakeWindowManager.RegisterHandler(model => AddTraceViewHandler(model, @"some title", "", Answer.Yes));
            ShellVm.ComplyWithRequest(new RequestAddTrace() { LastNodeId = d2, NodeWithRtuId = nodeForRtuId }).Wait();
            Poller.Tick();
        }

        public void CreateFieldForPathFinderTest(out Guid startId, out Guid finishId, out Guid wrongNodeId, out Guid wrongNodeWithEqId)
        {
            ShellVm.ComplyWithRequest(new AddRtuAtGpsLocation() {Latitude = 55, Longitude = 30}).Wait();
            Poller.Tick();
            startId = ReadModel.Rtus.Last().NodeId;

            ShellVm.ComplyWithRequest(new AddNode()).Wait(); Poller.Tick(); var b0 = ReadModel.Nodes.Last().Id;
            ShellVm.ComplyWithRequest(new AddNode()).Wait(); Poller.Tick(); var b1 = ReadModel.Nodes.Last().Id;
            ShellVm.ComplyWithRequest(new AddNode()).Wait(); Poller.Tick(); var b2 = ReadModel.Nodes.Last().Id;

            ShellVm.ComplyWithRequest(new AddNode()).Wait(); Poller.Tick(); var c0 = ReadModel.Nodes.Last().Id; wrongNodeId = c0;
            ShellVm.ComplyWithRequest(new AddNode()).Wait(); Poller.Tick(); var c1 = ReadModel.Nodes.Last().Id;
            ShellVm.ComplyWithRequest(new AddEquipmentAtGpsLocation() {Type = EquipmentType.Sleeve }).Wait(); var c2 = ReadModel.Nodes.Last().Id;

            ShellVm.ComplyWithRequest(new AddNode()).Wait(); Poller.Tick(); var d0 = ReadModel.Nodes.Last().Id;
            ShellVm.ComplyWithRequest(new AddNode()).Wait(); Poller.Tick(); var d1 = ReadModel.Nodes.Last().Id;
            ShellVm.ComplyWithRequest(new AddEquipmentAtGpsLocation() { Type = EquipmentType.Sleeve }).Wait(); var d2 = ReadModel.Nodes.Last().Id;

            ShellVm.ComplyWithRequest(new AddNode()).Wait(); Poller.Tick(); var e0 = ReadModel.Nodes.Last().Id;
            ShellVm.ComplyWithRequest(new AddNode()).Wait(); Poller.Tick(); var e1 = ReadModel.Nodes.Last().Id;
            ShellVm.ComplyWithRequest(new AddNode()).Wait(); Poller.Tick(); var e2 = ReadModel.Nodes.Last().Id;

            ShellVm.ComplyWithRequest(new AddEquipmentAtGpsLocation() {Type = EquipmentType.Terminal }).Wait(); Poller.Tick(); finishId = ReadModel.Nodes.Last().Id;

            ShellVm.ComplyWithRequest(new AddNode()).Wait(); Poller.Tick(); var zz = ReadModel.Nodes.Last().Id;
            ShellVm.ComplyWithRequest(new AddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait(); Poller.Tick(); var z2 = ReadModel.Nodes.Last().Id;
            wrongNodeWithEqId = z2;


            ShellVm.ComplyWithRequest(new AddFiber() {Id = Guid.NewGuid(), Node1 = startId, Node2 = b0}).Wait();
            Poller.Tick();

            ShellVm.ComplyWithRequest(new AddFiber() { Id = new Guid(), Node1 = startId, Node2 = b1 }).Wait();
            ShellVm.ComplyWithRequest(new AddFiber() { Id = new Guid(), Node1 = startId, Node2 = b2 }).Wait();
            Poller.Tick();

            ShellVm.ComplyWithRequest(new AddFiber() { Id = new Guid(), Node1 = c0, Node2 = b0 }).Wait();
            ShellVm.ComplyWithRequest(new AddFiber() { Id = new Guid(), Node1 = c1, Node2 = b1 }).Wait();
            ShellVm.ComplyWithRequest(new AddFiber() { Id = new Guid(), Node1 = c2, Node2 = b2 }).Wait();
            Poller.Tick();

            ShellVm.ComplyWithRequest(new AddFiber() { Id = new Guid(), Node1 = c0, Node2 = d0 }).Wait();
            ShellVm.ComplyWithRequest(new AddFiber() { Id = new Guid(), Node1 = c1, Node2 = d1 }).Wait();
            ShellVm.ComplyWithRequest(new AddFiber() { Id = new Guid(), Node1 = c2, Node2 = d2 }).Wait();
            Poller.Tick();

            ShellVm.ComplyWithRequest(new AddFiber() { Id = new Guid(), Node1 = e0, Node2 = d0 }).Wait();
            ShellVm.ComplyWithRequest(new AddFiber() { Id = new Guid(), Node1 = e1, Node2 = d1 }).Wait();
            ShellVm.ComplyWithRequest(new AddFiber() { Id = new Guid(), Node1 = e2, Node2 = d2 }).Wait();
            Poller.Tick();

            ShellVm.ComplyWithRequest(new AddFiber() { Id = new Guid(), Node1 = e2, Node2 = finishId }).Wait();

            ShellVm.ComplyWithRequest(new AddFiber() { Id = new Guid(), Node1 = zz, Node2 = z2 }).Wait();
            Poller.Tick();
        }

        public bool QuestionAnswer(string question, Answer answer, object model)
        {
            var vm = model as QuestionViewModel;
            if (vm == null) return false;
            if (vm.QuestionMessage != question) return false;
            switch (answer)
            {
                case Answer.Yes:
                    vm.OkButton();
                    return true;
                case Answer.Cancel:
                    vm.CancelButton();
                    return true;
                default:
                    return false;
            }
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
                    return true;
                case EquipmentChoiceAnswer.SetupNameAndContinue:
                    vm.Choices[chosenEquipmentNumber].IsChecked = true;
                    vm.SelectAndSetupNameButton();
                    return true;
                case EquipmentChoiceAnswer.Cancel:
                    vm.CancelButton();
                    return true;
                default:
                    return false;
            }
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

        public bool EquipmentUpdateHandler(object model, Guid nodeId, EquipmentType type, string title, string comment, int leftReserve, int rightReserve, Answer answer)
        {
            var vm = model as EquipmentUpdateViewModel;
            if (vm == null) return false;
            vm.NodeId = nodeId;
            vm.Type = type;
            vm.Title = title;
            vm.Comment = comment;
            vm.CableReserveLeft = leftReserve;
            vm.CableReserveRight = rightReserve;
            if (answer == Answer.Yes)
                vm.Save();
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