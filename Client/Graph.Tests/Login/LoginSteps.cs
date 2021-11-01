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
            _sut.ReadModel.Users.Count.ShouldBeEquivalentTo(0);
        }

        [When(@"Рут входит и выбирает применить Демо лицензию")]
        public void WhenРутВходитИВыбираетПрименитьДемоЛицензию()
        {
            _sut.LoginOnEmptyBaseAsRoot();
        }

        [Then(@"Вход осуществлен разрешен один пользователь и один рту")]
        public void ThenВходОсуществленРазрешенОдинПользовательИОдинРту()
        {
            _sut.ReadModel.Licenses.Count.ShouldBeEquivalentTo(1);
            var license = _sut.ReadModel.Licenses.First();
            license.RtuCount.Value.ShouldBeEquivalentTo(1);
            license.ClientStationCount.Value.ShouldBeEquivalentTo(1);
        }

        [Then(@"Рут выходит на этом же компе входит оператор")]
        public void ThenРутВыходитНаЭтомЖеКомпеВходитОператор()
        {
            _sut.LogoutAs("root");
            _sut.LoginAs("operator");
        }

        [When(@"Рут входит и указывает файл с лицензией без привязки рабмест")]
        public void WhenРутВходитИУказываетФайлСЛицензиейБезПривязкиРабмест()
        {
            _sut.LoginOnEmptyBaseAsRoot(SystemUnderTest.Fibertest20dev);
        }

        [Then(@"Вход осуществлен пользователи и рту разрешены в соответствии с лицензионным файлом")]
        public void ThenВходОсуществленПользователиИРтуРазрешеныВСоответствииСЛицензионнымФайлом()
        {
            _sut.ReadModel.Licenses.Count.ShouldBeEquivalentTo(1);
            var license = _sut.ReadModel.Licenses.First();
            license.RtuCount.Value.ShouldBeEquivalentTo(999);
            license.ClientStationCount.Value.ShouldBeEquivalentTo(5);
        }

        [When(@"Рут входит и указывает файл с лицензией с привязкой рабмест вводится пароль безопасника")]
        public void WhenРутВходитИУказываетФайлСЛицензиейСПривязкойРабместВводитсяПарольБезопасника()
        {
            _sut.LoginOnEmptyBaseAsRoot(SystemUnderTest.DevSecAdmin);
        }

        [Then(@"Вход осуществлен Рут привязан к машине Пользователи и рту разрешены кроме вэбклиентов")]
        public void ThenВходОсуществленРутПривязанКМашинеПользователиИРтуРазрешеныКромеВэбклиентов()
        {
            _sut.ReadModel.Licenses.Count.ShouldBeEquivalentTo(1);
            var license = _sut.ReadModel.Licenses.First();
            license.RtuCount.Value.ShouldBeEquivalentTo(999);
            license.ClientStationCount.Value.ShouldBeEquivalentTo(5);
            license.WebClientCount.Value.ShouldBeEquivalentTo(0);

            var writeModel = _sut.ServerScope.Resolve<Model>();
            writeModel.Users.First(u => u.Title == "root").MachineKey.ShouldBeEquivalentTo(new FakeMachineKeyProvider().Get());
        }

        [When(@"Рут выходит")]
        public void WhenРутВыходит()
        {
            _sut.LogoutAs("root");
        }

        [Then(@"Оператор входит выдает запрос пароля безопасника")]
        public void ThenОператорВходитВыдаетЗапросПароляБезопасника()
        {
            _sut.LoginAs("operator", "wrong_password");
        }

    }
}
