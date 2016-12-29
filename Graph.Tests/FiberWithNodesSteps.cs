using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Iit.Fibertest.Graph;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class FiberWithNodesSteps
    {
        private readonly SystemUnderTest _sut = new SystemUnderTest();
        private Guid _leftNodeId;
        private Guid _rightNodeId;
        private int _cutOff;

        [Given(@"Левый и правый узлы уже созданы")]
        public void GivenЛевыйИПравыйУзлыУжеСозданы()
        {
            _leftNodeId = _sut.AddNode();
            _cutOff = _sut.CurrentEventNumber;
            _rightNodeId = _sut.AddNode();
            _cutOff = _sut.CurrentEventNumber;
        }

        [Given(@"Между левым и правым узлом уже добавлен отрезок")]
        public void GivenМеждуЛевымИПравымУзломУжеДобавленОтрезок()
        {
            _sut.AddFiber(_leftNodeId, _rightNodeId);
            _cutOff = _sut.CurrentEventNumber;
        }

        [When(@"Пользователь кликает добавить отрезок с узлами")]
        public void WhenПользовательКликаетДобавитьОтрезокСУзлами()
        {
            const int neverMind = -1;
            const EquipmentType doesntMatter = EquipmentType.Other;
            _sut.AddFiberWithNodes(_leftNodeId, _rightNodeId, neverMind, doesntMatter);
        }

        [When(@"Пользователь кликает добавить отрезок с нулем узлов")]
        public void WhenПользовательКликаетДобавитьОтрезокСнулемУзлов()
        {
            const EquipmentType doesntMatter = EquipmentType.Other;
            _sut.AddFiberWithNodes(_leftNodeId, _rightNodeId, 0, doesntMatter);

        }


        [When(@"Пользователь кликает добавить отрезок с (.*) пустыми узлами")]
        public void WhenПользовательКликаетДобавитьОтрезокСПустымиУзлами(int p0)
        {
            _sut.AddFiberWithNodes(_leftNodeId, _rightNodeId, p0, EquipmentType.None);
        }

        [When(@"Пользователь кликает добавить отрезок с N узлов с оборудованием")]
        public void WhenПользовательКликаетДобавитьОтрезокСnУзловСОборудованием()
        {
            const int moreThan0 = 3;
            const EquipmentType anyButNone = EquipmentType.Other;
            _sut.AddFiberWithNodes(_leftNodeId, _rightNodeId, moreThan0, anyButNone);
        }


        [Then(@"Новый отрезок не сохраняется")]
        public void ThenНовыйОтрезокНеСохраняется()
        {
            _sut.CurrentEventNumber.Should().Be(_cutOff);
        }

        [Then(@"Новый отрезок сохраняется")]
        public void ThenНовыйОтрезокСохраняется()
        {
            _sut.CurrentEventNumber.Should().BeGreaterThan(_cutOff);
        }

        [Then(@"Сохраняется (.*) узла и (.*) отрезка")]
        public void ThenСохраняетсяУзлаИОтрезка(int p0, int p1)
        {
            _sut.CurrentEventNumber.Should().Be(_cutOff + p0 + p1);
        }

        [Then(@"Сохраняется N узлов N элементов оборудования и на один больше отрезков")]
        public void ThenСохраняетсяNУзловNЭлементовОборудованияИНаОдинБольшеОтрезков()
        {
            _sut.CurrentEventNumber.Should().Be(_cutOff);
        }

    }
}
