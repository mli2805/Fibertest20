using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Iit.Fibertest.TestBench;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class TraceAddedSecondStageSteps
    {
        private readonly SystemUnderTest2 _sut = new SystemUnderTest2();
        private Guid _rtuNodeId;
        private Guid _lastNodeId;

        private List<Guid> _nodes;
        private List<Guid> _equipments;

        [Given(@"После того как принял предложенный маршрут трассы")]
        public void GivenХотяПринялПредложенныйМаршрутТрассы()
        {
            _sut.CreateFieldForPathFinderTest(out _rtuNodeId, out _lastNodeId);
            new PathFinder(_sut.ReadModel).FindPath(_rtuNodeId, _lastNodeId, out _nodes);

            _equipments = MapTraceDefine.CollectEquipmentForTrace(_sut.FakeWindowManager, _nodes, _sut.ReadModel);
        }

        [Given(@"На предложение выбрать оборудование пользователь отвечает: ""(.*)""")]
        public void DefineEquipmentChoiceAnswer(EquipmentChoiceAnswer answer)
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.EquipmentChoiceHandler(model, answer, 0));
        }


        [Given(@"Пользователь делает выбор для каждого предложенного узла")]
        public void GivenПользовательДелаетВыборДляКаждогоПредложенногоУзла()
        {
            _sut.FakeWindowManager.RegisterHandler(model => _sut.EquipmentChoiceHandler(model, EquipmentChoiceAnswer.Continue, 0));
            _sut.FakeWindowManager.RegisterHandler(model => _sut.EquipmentChoiceHandler(model, EquipmentChoiceAnswer.SetupNameAndContinue, 0));
            _sut.FakeWindowManager.RegisterHandler(model => _sut.EquipmentChoiceHandler(model, EquipmentChoiceAnswer.Continue, 0));
        }

        [Then(@"Список оборудования для трассы содержит выбранное пользователем оборудование")]
        public void ThenСписокОборудованияДляТрассыСодержитВыбранноеПользователемОборудование()
        {
            _equipments.Count.Should().Be(_nodes.Count);
            for (int i = 0; i < _nodes.Count; i++)
            {
                if (i == 0)
                    _sut.ReadModel.Rtus.Single(r => r.Id == _equipments[i]).NodeId.Should().Be(_nodes[i]);
                else if (_equipments[i] != Guid.Empty)
                        _sut.ReadModel.Equipments.Single(e => e.Id == _equipments[i]).NodeId.Should().Be(_nodes[i]);
            }
        }
    }

}

