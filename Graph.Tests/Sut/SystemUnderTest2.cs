using System;
using System.Linq;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;
using Iit.Fibertest.TestBench;

namespace Graph.Tests
{
    public class SystemUnderTest2
    {
        public Aggregate Aggregate { get; }
        public ReadModel ReadModel { get; }
        public ClientPoller Poller { get; }
        public FakeWindowManager FakeWindowManager { get; }
        public ShellViewModel ShellVm { get; }
        public int CurrentEventNumber => Poller.CurrentEventNumber;
        public const string Path = @"..\..\Sut\base.sor";

        public SystemUnderTest2()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<AutofacEventSourcing>();
            builder.RegisterModule<AutofacUi>();
            builder.RegisterType<FakeWindowManager>().As<IWindowManager>().SingleInstance();

            var container = builder.Build();
            Poller = container.Resolve<ClientPoller>();
            FakeWindowManager =(FakeWindowManager) container.Resolve<IWindowManager>();
            Aggregate = container.Resolve<Aggregate>();
            ReadModel = container.Resolve<ReadModel>();
            ShellVm = (ShellViewModel) container.Resolve<IShell>();
        }

        public void CreateTraceRtuEmptyTerminal()
        {
            ShellVm.ComplyWithRequest(new AddRtuAtGpsLocation() {Latitude = 55, Longitude = 30}).Wait();
            Poller.Tick();
            var nodeForRtuId = ReadModel.Rtus.Last().NodeId;
            ShellVm.ComplyWithRequest(new AddNode()).Wait();
            ShellVm.ComplyWithRequest(new AddEquipmentAtGpsLocation() {Type = EquipmentType.Terminal}).Wait();
            Poller.Tick();
            var firstNodeId = ReadModel.Nodes[1].Id;
            var secondNodeId = ReadModel.Nodes.Last().Id;
            ShellVm.ComplyWithRequest(new AddFiber() {Node1 = nodeForRtuId, Node2 = firstNodeId}).Wait();
            ShellVm.ComplyWithRequest(new AddFiber() {Node1 = firstNodeId, Node2 = secondNodeId }).Wait();
            Poller.Tick();


            FakeWindowManager.RegisterHandler(model => QuestionAnswer("Accept the path?", Answer.Yes, model));
            FakeWindowManager.RegisterHandler(model => EquipmentChoiceHandler(EquipmentChoiceAnswer.Use, model));
            FakeWindowManager.RegisterHandler(model => AddTraceViewHandler(model, "some title", "", Answer.Yes));

            ShellVm.ComplyWithRequest(new AskAddTrace() { LastNodeId = secondNodeId, NodeWithRtuId = nodeForRtuId }).Wait();
            Poller.Tick();
        }

        public void CreatePositionForAddNodeIntoFiberTest()
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

            FakeWindowManager.RegisterHandler(model => QuestionAnswer("Accept the path?", Answer.Yes, model));
            FakeWindowManager.RegisterHandler(model => EquipmentChoiceHandler(EquipmentChoiceAnswer.Use, model));
            FakeWindowManager.RegisterHandler(model => AddTraceViewHandler(model, "some title", "", Answer.Yes));
            ShellVm.ComplyWithRequest(new AskAddTrace() { LastNodeId = a2, NodeWithRtuId = nodeForRtuId }).Wait();
            Poller.Tick();

            FakeWindowManager.RegisterHandler(model => QuestionAnswer("Accept the path?", Answer.Yes, model));
            FakeWindowManager.RegisterHandler(model => EquipmentChoiceHandler(EquipmentChoiceAnswer.Use, model));
            FakeWindowManager.RegisterHandler(model => AddTraceViewHandler(model, "some title", "", Answer.Yes));
            ShellVm.ComplyWithRequest(new AskAddTrace() { LastNodeId = b2, NodeWithRtuId = nodeForRtuId }).Wait();
            Poller.Tick();

            FakeWindowManager.RegisterHandler(model => QuestionAnswer("Accept the path?", Answer.Yes, model));
            FakeWindowManager.RegisterHandler(model => EquipmentChoiceHandler(EquipmentChoiceAnswer.Use, model));
            FakeWindowManager.RegisterHandler(model => AddTraceViewHandler(model, "some title", "", Answer.Yes));
            ShellVm.ComplyWithRequest(new AskAddTrace() { LastNodeId = c2, NodeWithRtuId = nodeForRtuId }).Wait();
            Poller.Tick();

            FakeWindowManager.RegisterHandler(model => QuestionAnswer("Accept the path?", Answer.Yes, model));
            FakeWindowManager.RegisterHandler(model => EquipmentChoiceHandler(EquipmentChoiceAnswer.Use, model));
            FakeWindowManager.RegisterHandler(model => AddTraceViewHandler(model, "some title", "", Answer.Yes));
            ShellVm.ComplyWithRequest(new AskAddTrace() { LastNodeId = d2, NodeWithRtuId = nodeForRtuId }).Wait();
            Poller.Tick();
        }

        public void CreateFieldForPathFinderTest(out Guid startId, out Guid finishId)
        {
            ShellVm.ComplyWithRequest(new AddRtuAtGpsLocation() {Latitude = 55, Longitude = 30}).Wait();
            Poller.Tick();
            startId = ReadModel.Rtus.Last().NodeId;

            var b0 = new Iit.Fibertest.Graph.Node() { Id = Guid.NewGuid(), Title = "b0" };
            ReadModel.Nodes.Add(b0);
            var b1 = new Iit.Fibertest.Graph.Node() { Id = Guid.NewGuid(), Title = "b1" };
            ReadModel.Nodes.Add(b1);
            var b2 = new Iit.Fibertest.Graph.Node() { Id = Guid.NewGuid(), Title = "b2" };
            ReadModel.Nodes.Add(b2);
            var c0 = new Iit.Fibertest.Graph.Node() { Id = Guid.NewGuid(), Title = "c0" };
            ReadModel.Nodes.Add(c0);
            var c1 = new Iit.Fibertest.Graph.Node() { Id = Guid.NewGuid(), Title = "c1" };
            ReadModel.Nodes.Add(c1);
            var c2 = new Iit.Fibertest.Graph.Node() { Id = Guid.NewGuid(), Title = "c2" };
            ReadModel.Nodes.Add(c2);
            var d0 = new Iit.Fibertest.Graph.Node() { Id = Guid.NewGuid(), Title = "d0" };
            ReadModel.Nodes.Add(d0);
            var d1 = new Iit.Fibertest.Graph.Node() { Id = Guid.NewGuid(), Title = "d1" };
            ReadModel.Nodes.Add(d1);
            var d2 = new Iit.Fibertest.Graph.Node() { Id = Guid.NewGuid(), Title = "d2" };
            ReadModel.Nodes.Add(d2);
            var e0 = new Iit.Fibertest.Graph.Node() { Id = Guid.NewGuid(), Title = "e0" };
            ReadModel.Nodes.Add(e0);
            var e1 = new Iit.Fibertest.Graph.Node() { Id = Guid.NewGuid(), Title = "e1" };
            ReadModel.Nodes.Add(e1);
            var e2 = new Iit.Fibertest.Graph.Node() { Id = Guid.NewGuid(), Title = "e2" };
            ReadModel.Nodes.Add(e2);

            var nn = new Iit.Fibertest.Graph.Node() { Id = Guid.NewGuid(), Title = "nn" };
            ReadModel.Nodes.Add(nn);
            finishId = nn.Id;
            ReadModel.Equipments.Add(new Iit.Fibertest.Graph.Equipment() { Id = Guid.NewGuid(), NodeId = finishId, Title = "Terminal", Type = EquipmentType.Terminal });

            var zz = new Iit.Fibertest.Graph.Node() { Id = Guid.NewGuid(), Title = "zz" };
            ReadModel.Nodes.Add(zz);
            var z2 = new Iit.Fibertest.Graph.Node() { Id = Guid.NewGuid(), Title = "z2" };
            ReadModel.Nodes.Add(z2);

            ReadModel.Equipments.Add(new Iit.Fibertest.Graph.Equipment() { Id = Guid.NewGuid(), NodeId = c2.Id, Title = "Some sleeve", Type = EquipmentType.Sleeve });
            ReadModel.Equipments.Add(new Iit.Fibertest.Graph.Equipment() { Id = Guid.NewGuid(), NodeId = c2.Id, Title = "Some cross", Type = EquipmentType.Cross });

            ReadModel.Equipments.Add(new Iit.Fibertest.Graph.Equipment() { Id = Guid.NewGuid(), NodeId = d2.Id, Title = "Another sleeve", Type = EquipmentType.Sleeve });


            ReadModel.Fibers.Add(new Iit.Fibertest.Graph.Fiber() { Id = new Guid(), Node1 = startId, Node2 = b0.Id });
            ReadModel.Fibers.Add(new Iit.Fibertest.Graph.Fiber() { Id = new Guid(), Node1 = startId, Node2 = b1.Id });
            ReadModel.Fibers.Add(new Iit.Fibertest.Graph.Fiber() { Id = new Guid(), Node1 = startId, Node2 = b2.Id });

            ReadModel.Fibers.Add(new Iit.Fibertest.Graph.Fiber() { Id = new Guid(), Node1 = c0.Id, Node2 = b0.Id });
            ReadModel.Fibers.Add(new Iit.Fibertest.Graph.Fiber() { Id = new Guid(), Node1 = c1.Id, Node2 = b1.Id });
            ReadModel.Fibers.Add(new Iit.Fibertest.Graph.Fiber() { Id = new Guid(), Node1 = c2.Id, Node2 = b2.Id });

            ReadModel.Fibers.Add(new Iit.Fibertest.Graph.Fiber() { Id = new Guid(), Node1 = c0.Id, Node2 = d0.Id });
            ReadModel.Fibers.Add(new Iit.Fibertest.Graph.Fiber() { Id = new Guid(), Node1 = c1.Id, Node2 = d1.Id });
            ReadModel.Fibers.Add(new Iit.Fibertest.Graph.Fiber() { Id = new Guid(), Node1 = c2.Id, Node2 = d2.Id });

            ReadModel.Fibers.Add(new Iit.Fibertest.Graph.Fiber() { Id = new Guid(), Node1 = e0.Id, Node2 = d0.Id });
            ReadModel.Fibers.Add(new Iit.Fibertest.Graph.Fiber() { Id = new Guid(), Node1 = e1.Id, Node2 = d1.Id });
            ReadModel.Fibers.Add(new Iit.Fibertest.Graph.Fiber() { Id = new Guid(), Node1 = e2.Id, Node2 = d2.Id });

            ReadModel.Fibers.Add(new Iit.Fibertest.Graph.Fiber() { Id = new Guid(), Node1 = e2.Id, Node2 = nn.Id });

            ReadModel.Fibers.Add(new Iit.Fibertest.Graph.Fiber() { Id = new Guid(), Node1 = zz.Id, Node2 = z2.Id });
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

        public bool EquipmentChoiceHandler(EquipmentChoiceAnswer answer, object model)
        {
            var vm = model as EquipmentChoiceViewModel;
            if (vm == null) return false;
            switch (answer)
            {
                case EquipmentChoiceAnswer.Use:
                    vm.UseButton();
                    return true;
                case EquipmentChoiceAnswer.UseAndSetupName:
                    vm.UseAndSetupNameButton();
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
            var vm = model as TraceAddViewModel;
            if (vm == null) return false;
            vm.Title = title;
            vm.Comment = comment;
            if (button == Answer.Yes)
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
                vm.Ok();
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

        public bool BaseRefAssignHandler(object model, string preciseFilename, string fastFilename, string additionalFilename, Answer answer)
        {
            var vm = model as BaseRefsAssignViewModel;
            if (vm == null) return false;
            if (preciseFilename != null)
                vm.PreciseBaseFilename = preciseFilename;
            if (fastFilename != null)
                vm.FastBaseFilename = fastFilename;
            if (additionalFilename != null)
                vm.AdditionalBaseFilename = additionalFilename;
            if (answer == Answer.Yes)
                vm.Save();
            else
                vm.Cancel();
            return true;
        }
    }

}