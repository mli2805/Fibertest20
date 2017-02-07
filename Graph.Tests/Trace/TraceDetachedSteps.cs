﻿using System;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.Graph.Commands;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class TraceDetachedSteps
    {
        private readonly SystemUnderTest2 _sut;
        private Guid _traceId;

        public TraceDetachedSteps(SystemUnderTest2 sut)
        {
            _sut = sut;
        }

        [Given(@"Трасса присоединена к порту РТУ")]
        public void GivenТрассаПрисоединенаКПортуРту()
        {
            _traceId = _sut.ReadModel.Traces.Single().Id;
            var cmd2 = new AttachTrace()
            {
                Port = 3,
                TraceId = _traceId
            };
            _sut.ShellVm.ComplyWithRequest(cmd2).Wait();
            _sut.Poller.Tick();
        }

        [When(@"Пользователь отсоединяет трассу")]
        public void WhenПользовательОтсоединяетТрассу()
        {
            var cmd = new DetachTrace() {TraceId = _traceId};
            _sut.ShellVm.ComplyWithRequest(cmd).Wait();
            _sut.Poller.Tick();
        }

        [Then(@"Трасса отсоединена")]
        public void ThenТрассаОтсоединена()
        {
            _sut.ReadModel.Traces.Single(t => t.Id == _traceId).Port.Should().Be(-1);
        }

    }
}
