﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:2.4.0.0
//      SpecFlow Generator Version:2.4.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace Graph.Tests.Rtu
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.4.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public partial class RtuInitializedFeature : Xunit.IClassFixture<RtuInitializedFeature.FixtureData>, System.IDisposable
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private Xunit.Abstractions.ITestOutputHelper _testOutputHelper;
        
#line 1 "RtuInitialized.feature"
#line hidden
        
        public RtuInitializedFeature(RtuInitializedFeature.FixtureData fixtureData, Xunit.Abstractions.ITestOutputHelper testOutputHelper)
        {
            this._testOutputHelper = testOutputHelper;
            this.TestInitialize();
        }
        
        public static void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "RtuInitialized", null, ProgrammingLanguage.CSharp, ((string[])(null)));
            testRunner.OnFeatureStart(featureInfo);
        }
        
        public static void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        public virtual void TestInitialize()
        {
        }
        
        public virtual void ScenarioTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioInitialize(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioInitialize(scenarioInfo);
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<Xunit.Abstractions.ITestOutputHelper>(_testOutputHelper);
        }
        
        public virtual void ScenarioStart()
        {
            testRunner.OnScenarioStart();
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        public virtual void FeatureBackground()
        {
#line 3
#line 4
 testRunner.Given("Пустая база входит рут и применяет демо лицензию", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 5
 testRunner.Given("Существует RTU с основным 192.168.96.52 и резервным 172.16.4.10 адресами", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 6
 testRunner.Given("Создан еще РТУ даже с трассой", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
        }
        
        void System.IDisposable.Dispose()
        {
            this.ScenarioTearDown();
        }
        
        [Xunit.FactAttribute(DisplayName="Пользователь отказывается инициализировать РТУ")]
        [Xunit.TraitAttribute("FeatureTitle", "RtuInitialized")]
        [Xunit.TraitAttribute("Description", "Пользователь отказывается инициализировать РТУ")]
        public virtual void ПользовательОтказываетсяИнициализироватьРТУ()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Пользователь отказывается инициализировать РТУ", null, ((string[])(null)));
#line 8
this.ScenarioInitialize(scenarioInfo);
            this.ScenarioStart();
#line 3
this.FeatureBackground();
#line 9
 testRunner.When("Пользователь открывает форму инициализации и жмет Отмена", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 10
 testRunner.Then("РТУ НЕ инициализирован", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Xunit.FactAttribute(DisplayName="Запрещено инициализировать больше RTU чем указано в лицензии")]
        [Xunit.TraitAttribute("FeatureTitle", "RtuInitialized")]
        [Xunit.TraitAttribute("Description", "Запрещено инициализировать больше RTU чем указано в лицензии")]
        public virtual void ЗапрещеноИнициализироватьБольшеRTUЧемУказаноВЛицензии()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Запрещено инициализировать больше RTU чем указано в лицензии", null, ((string[])(null)));
#line 12
this.ScenarioInitialize(scenarioInfo);
            this.ScenarioStart();
#line 3
this.FeatureBackground();
#line 13
 testRunner.When("Пользователь вводит основной адрес 192.168.96.55 и жмет Инициализировать", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 14
 testRunner.Then("Выдается сообщение о превышеном лимите", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 15
 testRunner.When("На сервере применена другая лицензия с двумя RTU", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 16
 testRunner.When("Пользователь вводит основной адрес 192.168.96.55 и жмет Инициализировать", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 17
 testRunner.Then("RTU инициализирован только с основным адресом", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Xunit.FactAttribute(DisplayName="Запрещено инициализировать RTU с уже используемым адресом")]
        [Xunit.TraitAttribute("FeatureTitle", "RtuInitialized")]
        [Xunit.TraitAttribute("Description", "Запрещено инициализировать RTU с уже используемым адресом")]
        public virtual void ЗапрещеноИнициализироватьRTUСУжеИспользуемымАдресом()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Запрещено инициализировать RTU с уже используемым адресом", null, ((string[])(null)));
#line 19
this.ScenarioInitialize(scenarioInfo);
            this.ScenarioStart();
#line 3
this.FeatureBackground();
#line 20
 testRunner.When("На сервере применена другая лицензия с двумя RTU", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 21
 testRunner.When("Пользователь задает имя RTU", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 22
 testRunner.When("Пользователь вводит основной адрес 192.168.96.52 и жмет Инициализировать", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 23
 testRunner.Then("Сообщение об существовании RTU с таким адресом", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 24
 testRunner.When("Пользователь вводит основной адрес 172.16.4.10 и жмет Инициализировать", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 25
 testRunner.Then("Сообщение об существовании RTU с таким адресом", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 26
 testRunner.When("Пользователь вводит основной 10.100.1.41 и резервный 192.168.96.52 адреса и жмет " +
                    "Инициализировать", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 27
 testRunner.Then("Сообщение об существовании RTU с таким адресом", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 28
 testRunner.When("Пользователь вводит основной 10.100.1.41 и резервный 172.16.4.10 адреса и жмет Ин" +
                    "ициализировать", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 29
 testRunner.Then("Сообщение об существовании RTU с таким адресом", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 31
 testRunner.When("Пользователь вводит основной адрес 192.168.96.55 и жмет Инициализировать", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 32
 testRunner.Then("RTU инициализирован только с основным адресом", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 34
 testRunner.When("Пользователь вводит основной 10.100.1.41 и резервный 10.100.12.34 адреса и жмет И" +
                    "нициализировать", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 35
 testRunner.Then("RTU инициализирован с основным и резервным адресами", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.4.0.0")]
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
        public class FixtureData : System.IDisposable
        {
            
            public FixtureData()
            {
                RtuInitializedFeature.FeatureSetup();
            }
            
            void System.IDisposable.Dispose()
            {
                RtuInitializedFeature.FeatureTearDown();
            }
        }
    }
}
#pragma warning restore
#endregion

