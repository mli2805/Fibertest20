using System.Linq;
using Autofac;
using FluentAssertions;
using Iit.Fibertest.Graph;
using TechTalk.SpecFlow;

namespace Graph.Tests
{
    [Binding]
    public sealed class LoginSteps
    {
        private SystemUnderTest _sut = new SystemUnderTest();

        [Given(@"База пустая после установки или сброса")]
        public void GivenБазаПустаяПослеУстановкиИлиСброса()
        {
            var writeModel = _sut.ServerScope.Resolve<Model>();
            writeModel.Licenses.Count.ShouldBeEquivalentTo(0);
        }

        [When(@"Рут входит и выбирает применить Демо лицензию")]
        public void WhenРутВходитИВыбираетПрименитьДемоЛицензию()
        {
            _sut.ReadModel.Users.Count.ShouldBeEquivalentTo(0);
            _sut.LoginAsRoot(withDemoLicense: Answer.Yes);
        }

        [Then(@"Вход осуществлен разрешен один пользователь и один рту")]
        public void ThenВходОсуществленРазрешенОдинПользовательИОдинРту()
        {
            _sut.ReadModel.Licenses.Count.ShouldBeEquivalentTo(1);
            var license = _sut.ReadModel.Licenses.First();
            license.RtuCount.Value.ShouldBeEquivalentTo(1);
            license.ClientStationCount.Value.ShouldBeEquivalentTo(1);
        }
    }
}
