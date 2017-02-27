using System;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class TraceCleanedSteps
    {
        SutForTraceCleanRemove _sut = new SutForTraceCleanRemove();
        private Guid attachedTrace, notAttachedTrace;

        [Given(@"Даны две трассы с общим отрезком")]
        public void GivenДаныДвеТрассыСОбщимОтрезком()
        {
            _sut.CreateTwoTraces(out attachedTrace, out notAttachedTrace);
        }

        [Given(@"Одна из трасс присоединена к порту")]
        public void GivenОднаИзТрассПрисоединенаКПорту()
        {
        
        }
    }
}
