﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:2.1.0.0
//      SpecFlow Generator Version:2.0.0.0
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
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.1.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public partial class RtuInitializedFeature : Xunit.IClassFixture<RtuInitializedFeature.FixtureData>, System.IDisposable
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "RtuInitialized.feature"
#line hidden
        
        public RtuInitializedFeature()
        {
            this.TestInitialize();
        }
        
        public static void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "RtuInitializedDto", null, ProgrammingLanguage.CSharp, ((string[])(null)));
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
        
        public virtual void ScenarioSetup(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioStart(scenarioInfo);
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        public virtual void FeatureBackground()
        {
#line 3
#line 4
 testRunner.Given("Существует RTU с основным 192.168.96.52 и резервным 172.16.4.10 адресами", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 5
 testRunner.Given("Создан РТУ даже с трассой", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
        }
        
        public virtual void SetFixture(RtuInitializedFeature.FixtureData fixtureData)
        {
        }
        
        void System.IDisposable.Dispose()
        {
            this.ScenarioTearDown();
        }
        
        [Xunit.FactAttribute(DisplayName="Запрещено инициализировать RTU с используемым адресом")]
        [Xunit.TraitAttribute("FeatureTitle", "RtuInitializedDto")]
        [Xunit.TraitAttribute("Description", "Запрещено инициализировать RTU с используемым адресом")]
        public virtual void ЗапрещеноИнициализироватьRTUСИспользуемымАдресом()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Запрещено инициализировать RTU с используемым адресом", ((string[])(null)));
#line 7
this.ScenarioSetup(scenarioInfo);
#line 3
this.FeatureBackground();
#line 8
 testRunner.When("Пользователь вводит основной адрес 192.168.96.52 и жмет Инициализировать", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 9
 testRunner.Then("Сообщение об существовании RTU с таким адресом", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 10
 testRunner.When("Пользователь вводит основной адрес 172.16.4.10 и жмет Инициализировать", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 11
 testRunner.Then("Сообщение об существовании RTU с таким адресом", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 12
 testRunner.When("Пользователь вводит основной 10.100.1.41 и резервный 192.168.96.52 адреса и жмет " +
                    "Инициализировать", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 13
 testRunner.Then("Сообщение об существовании RTU с таким адресом", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 14
 testRunner.When("Пользователь вводит основной 10.100.1.41 и резервный 172.16.4.10 адреса и жмет Ин" +
                    "ициализировать", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 15
 testRunner.Then("Сообщение об существовании RTU с таким адресом", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Xunit.FactAttribute(DisplayName="Пользователь инициализирует RTU только с основным адресом")]
        [Xunit.TraitAttribute("FeatureTitle", "RtuInitializedDto")]
        [Xunit.TraitAttribute("Description", "Пользователь инициализирует RTU только с основным адресом")]
        public virtual void ПользовательИнициализируетRTUТолькоСОсновнымАдресом()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Пользователь инициализирует RTU только с основным адресом", ((string[])(null)));
#line 17
this.ScenarioSetup(scenarioInfo);
#line 3
this.FeatureBackground();
#line 18
 testRunner.When("Пользователь вводит основной адрес 192.168.96.55 и жмет Инициализировать", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 19
 testRunner.Then("RTU инициализирован только с основным адресом", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Xunit.FactAttribute(DisplayName="Пользователь инициализирует RTU с основным и резервным адресами")]
        [Xunit.TraitAttribute("FeatureTitle", "RtuInitializedDto")]
        [Xunit.TraitAttribute("Description", "Пользователь инициализирует RTU с основным и резервным адресами")]
        public virtual void ПользовательИнициализируетRTUСОсновнымИРезервнымАдресами()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Пользователь инициализирует RTU с основным и резервным адресами", ((string[])(null)));
#line 21
this.ScenarioSetup(scenarioInfo);
#line 3
this.FeatureBackground();
#line 22
 testRunner.When("Пользователь вводит основной 10.100.1.41 и резервный 10.100.12.34 адреса и жмет И" +
                    "нициализировать", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 23
 testRunner.Then("RTU инициализирован с основным и резервным адресами", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Xunit.FactAttribute(DisplayName="Пользователь отказывается инициализировать РТУ")]
        [Xunit.TraitAttribute("FeatureTitle", "RtuInitializedDto")]
        [Xunit.TraitAttribute("Description", "Пользователь отказывается инициализировать РТУ")]
        public virtual void ПользовательОтказываетсяИнициализироватьРТУ()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Пользователь отказывается инициализировать РТУ", ((string[])(null)));
#line 25
this.ScenarioSetup(scenarioInfo);
#line 3
this.FeatureBackground();
#line 26
 testRunner.When("Пользователь открывает форму инициализации и жмет Отмена", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 27
 testRunner.Then("РТУ НЕ инициализирован", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.1.0.0")]
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
