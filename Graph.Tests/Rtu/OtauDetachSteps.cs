using System;
using Iit.Fibertest.TestBench;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class OtauDetachSteps
    {
        private SutForTraceAttach _sut = new SutForTraceAttach();
        private Guid _rtuId;
        private Iit.Fibertest.Graph.Trace _trace;
        private RtuLeaf _rtuLeaf;
        private OtauLeaf _otauLeaf;
        private int portNumber = 3;


        [Given(@"Существует инициализированый RTU с неприсоединенной трассой")]
        public void GivenСуществуетИнициализированыйRtuсНеприсоединеннойТрассой()
        {
            _trace = _sut.CreateTraceRtuEmptyTerminal();
            _rtuId = _trace.RtuId;
            _rtuLeaf = _sut.InitializeRtu(_rtuId);
        }

        [Given(@"К RTU подключен доп оптический переключатель")]
        public void GivenКrtuПодключенДопОптическийПереключатель()
        {
            _otauLeaf = _sut.AttachOtauToRtu(_rtuLeaf, portNumber);
        }
    }
}
