﻿using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using FluentAssertions;
using Iit.Fibertest.Graph.Commands;
using Iit.Fibertest.WpfClient.ViewModels;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class TraceDetachedSteps
    {
        private readonly SystemUnderTest _sut;
        private Guid _traceId;
        private int _portNumber;

        private Guid _nodeForRtuId;
        private Guid _firstNodeId;

        public TraceDetachedSteps(SystemUnderTest sut)
        {
            _sut = sut;
        }

        [Given(@"Трасса присоединена к порту РТУ")]
        public void GivenТрассаПрисоединенаКПортуРТУ()
        {
            _traceId = _sut.ReadModel.Traces.Single().Id;
            var cmd2 = new AttachTrace()
            {
                Port = 3,
                TraceId = _traceId
            };
            _sut.Map.AttachTrace(cmd2);
            _sut.Poller.Tick();
        }

        [When(@"Пользователь отсоединяет трассу")]
        public void WhenПользовательОтсоединяетТрассу()
        {
            var cmd = new DetachTrace() {TraceId = _traceId};
            _sut.Map.DetachTrace(cmd);
            _sut.Poller.Tick();
        }

        [Then(@"Трасса отсоединена")]
        public void ThenТрассаОтсоединена()
        {
            _sut.ReadModel.Traces.Single(t => t.Id == _traceId).Port.Should().Be(-1);
        }

    }
}
