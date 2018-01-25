﻿using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Client;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.DataCenterCore;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;
using Serilog;

namespace Graph.Tests
{
    public class SystemUnderTest
    {
        public ReadModel ReadModel { get; }
        public ILogger LoggerForTests { get; set; }
        public IMyLog MyLogFile { get; set; }
        public ClientPoller Poller { get; }
        public FakeWindowManager FakeWindowManager { get; }
        public WcfServiceForClient WcfServiceForClient { get; }
        public RtuLeafActions RtuLeafActions { get; }
        public TraceLeafActions TraceLeafActions { get; }
        public TraceLeafActionsPermissions TraceLeafActionsPermissions { get; }
        public PortLeafActions PortLeafActions { get; }
        public CommonActions CommonActions { get; }
        public ShellViewModel ShellVm { get; }
        public int CurrentEventNumber => Poller.CurrentEventNumber;
        public const string Base1625 = @"..\..\Sut\BaseRefs\base1625.sor";
        public const string BaseAnother1625 = @"..\..\Sut\BaseRefs\anotherBase1625.sor";
        public const string Base1550Lm3 = @"..\..\Sut\BaseRefs\baseRtuCrossTerminal1550.sor";

        public SystemUnderTest()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<AutofacClient>();
            builder.RegisterType<FakeWindowManager>().As<IWindowManager>().SingleInstance();
            builder.RegisterType<FakeLocalDbManager>().As<ILocalDbManager>().SingleInstance();
            builder.RegisterType<FakeClientWcfServiceHost>().As<IClientWcfServiceHost>();
            builder.RegisterType<FakeWaitCursor>().As<ICursorBlah>().SingleInstance();

            builder.RegisterType<FakeEventStoreInitializer>().As<IEventStoreInitializer>().SingleInstance();
            builder.RegisterType<EventStoreService>().SingleInstance();

            builder.RegisterType<ClientStationsRepository>().SingleInstance();
            builder.RegisterType<RtuStationsRepository>().SingleInstance();
            builder.RegisterType<ClientToRtuTransmitter>().SingleInstance();
            builder.RegisterType<BaseRefsRepository>().SingleInstance();
            builder.RegisterType<BaseRefsBusinessToRepositoryIntermediary>().SingleInstance();
            builder.RegisterType<MeasurementsRepository>().SingleInstance();
            builder.RegisterType<NetworkEventsRepository>().SingleInstance();
            builder.RegisterType<BopNetworkEventsRepository>().SingleInstance();
            builder.RegisterType<GraphPostProcessingRepository>().SingleInstance();
            builder.RegisterType<WcfServiceForClient>().As<IWcfServiceForClient>().SingleInstance();

            builder.RegisterInstance(LoggerForTests = new LoggerConfiguration()
                .WriteTo.Console().CreateLogger()).As<ILogger>();

            builder.RegisterInstance<IMyLog>(new NullLog());
            
            builder.RegisterType<TestsDispatcherProvider>().As<IDispatcherProvider>().SingleInstance();

            var container = builder.Build();

            Poller = container.Resolve<ClientPoller>();
            FakeWindowManager = (FakeWindowManager)container.Resolve<IWindowManager>();
            MyLogFile = container.Resolve<IMyLog>();
            WcfServiceForClient = (WcfServiceForClient) container.Resolve<IWcfServiceForClient>();
            ShellVm = (ShellViewModel)container.Resolve<IShell>();
            ReadModel = ShellVm.ReadModel;
            RtuLeafActions = container.Resolve<RtuLeafActions>();
            TraceLeafActions = container.Resolve<TraceLeafActions>();
            TraceLeafActionsPermissions = container.Resolve<TraceLeafActionsPermissions>();
            PortLeafActions = container.Resolve<PortLeafActions>();
            CommonActions = container.Resolve<CommonActions>();

            var ev = container.Resolve<EventStoreService>();
            ev.Init();
        }

        public Iit.Fibertest.Graph.Trace CreateTraceRtuEmptyTerminal(string title = @"some title")
        {
            ShellVm.ComplyWithRequest(new RequestAddRtuAtGpsLocation() { Latitude = 55, Longitude = 30 }).Wait();
            Poller.EventSourcingTick().Wait();
            var nodeForRtuId = ReadModel.Rtus.Last().NodeId;

            ShellVm.ComplyWithRequest(new RequestAddEquipmentAtGpsLocation(){Type = EquipmentType.EmptyNode}).Wait();
            Poller.EventSourcingTick().Wait();
            var firstNodeId = ReadModel.Nodes.Last().Id;

            ShellVm.ComplyWithRequest(new RequestAddEquipmentAtGpsLocation() { Type = EquipmentType.Terminal }).Wait();
            Poller.EventSourcingTick().Wait();
            var secondNodeId = ReadModel.Nodes.Last().Id;

            ShellVm.ComplyWithRequest(new AddFiber() { Node1 = nodeForRtuId, Node2 = firstNodeId }).Wait();
            ShellVm.ComplyWithRequest(new AddFiber() { Node1 = firstNodeId, Node2 = secondNodeId }).Wait();
            Poller.EventSourcingTick().Wait();

            return DefineTrace(secondNodeId, nodeForRtuId, title);
        }


        protected Iit.Fibertest.Graph.Trace DefineTrace(Guid lastNodeId, Guid nodeForRtuId, string title = @"some title")
        {
            FakeWindowManager.RegisterHandler(model => OneLineMessageBoxAnswer(Resources.SID_Accept_the_path, Answer.Yes, model));
            FakeWindowManager.RegisterHandler(model => TraceContentChoiceHandler(model, Answer.Yes, 0));
            FakeWindowManager.RegisterHandler(model => AddTraceViewHandler(model, title, "", Answer.Yes));
            ShellVm.ComplyWithRequest(new RequestAddTrace() { LastNodeId = lastNodeId, NodeWithRtuId = nodeForRtuId });
            Poller.EventSourcingTick().Wait();
            return ShellVm.ReadModel.Traces.Last();
        }


        public bool OneLineMessageBoxAnswer(string question, Answer answer, object model)
        {
            if (!(model is MyMessageBoxViewModel vm)) return false;
            if (vm.Lines[0].Line != question) return false;
            if (answer == Answer.Yes)
                vm.OkButton();
            else
                vm.CancelButton();
            return true;
        }

        public bool ManyLinesMessageBoxAnswer(Answer answer, object model)
        {
            if (!(model is MyMessageBoxViewModel vm)) return false;
            if (answer == Answer.Yes)
                vm.OkButton();
            else
                vm.CancelButton();
            return true;
        }

        public bool ManyLinesMessageBoxContainsStringAnswer(string str, Answer answer, object model)
        {
            if (!(model is MyMessageBoxViewModel vm))
                return false;
            if (vm.Lines.FirstOrDefault(i => i.Line == str) == null)
                return false;
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

        public bool TraceContentChoiceHandler(object model, Answer button, int selectedOptionNumber)
        {
            var vm = model as TraceContentChoiceViewModel;
            if (vm == null) return false;
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

        public bool AddTraceViewHandler(object model, string title, string comment, Answer button)
        {
            var vm = model as TraceInfoViewModel;
            if (vm == null) return false;
            vm.Model.Title = title;
            vm.Model.Comment = comment;
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