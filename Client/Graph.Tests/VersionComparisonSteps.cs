using FluentAssertions;
using Iit.Fibertest.Graph;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class VersionComparisonSteps
    {
        private string _v1;
        private string _v2;

        [Given(@"первый номер версии ""(.*)""")]
        public void GivenПервыйНомерВерсии_(string v1)
        {
            _v1 = v1;
        }

        [Given(@"второй номер версии ""(.*)""")]
        public void GivenВторойНомерВерсии(string v2)
        {
            _v2 = v2;
        }

        [Then(@"сравнение первый старше второй должно вернуть True")]
        public void ThenСравнениеПервыйСтаршеВторойДолжноВернутьTrue()
        {
            _v1.IsOlder(_v2).Should().BeTrue();
        }

    }
}
