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
    public partial class VeexRtuFirstInitializationFeature : Xunit.IClassFixture<VeexRtuFirstInitializationFeature.FixtureData>, System.IDisposable
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private Xunit.Abstractions.ITestOutputHelper _testOutputHelper;
        
#line 1 "VeexRtuFirstInitialization.feature"
#line hidden
        
        public VeexRtuFirstInitializationFeature(VeexRtuFirstInitializationFeature.FixtureData fixtureData, Xunit.Abstractions.ITestOutputHelper testOutputHelper)
        {
            this._testOutputHelper = testOutputHelper;
            this.TestInitialize();
        }
        
        public static void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "VeexRtuFirstInitialization", null, ProgrammingLanguage.CSharp, ((string[])(null)));
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
#line 2
#line 3
 testRunner.Given("На карте есть RTU", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
        }
        
        void System.IDisposable.Dispose()
        {
            this.ScenarioTearDown();
        }
        
        [Xunit.FactAttribute(DisplayName="Начальная инициализация Veex RTU с поломанным Otau")]
        [Xunit.TraitAttribute("FeatureTitle", "VeexRtuFirstInitialization")]
        [Xunit.TraitAttribute("Description", "Начальная инициализация Veex RTU с поломанным Otau")]
        public virtual void НачальнаяИнициализацияVeexRTUСПоломаннымOtau()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Начальная инициализация Veex RTU с поломанным Otau", null, ((string[])(null)));
#line 5
this.ScenarioInitialize(scenarioInfo);
            this.ScenarioStart();
#line 2
this.FeatureBackground();
#line 6
 testRunner.Given("У RTU один поломанный основной переключатель", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 7
 testRunner.When("Первая инициализация этого RTU", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 8
 testRunner.Then("В дереве у RTU портов - 1", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Xunit.FactAttribute(DisplayName="Начальная инициализация Veex RTU с 3 недоступными Otau")]
        [Xunit.TraitAttribute("FeatureTitle", "VeexRtuFirstInitialization")]
        [Xunit.TraitAttribute("Description", "Начальная инициализация Veex RTU с 3 недоступными Otau")]
        public virtual void НачальнаяИнициализацияVeexRTUС3НедоступнымиOtau()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Начальная инициализация Veex RTU с 3 недоступными Otau", null, ((string[])(null)));
#line 10
this.ScenarioInitialize(scenarioInfo);
            this.ScenarioStart();
#line 2
this.FeatureBackground();
#line 11
 testRunner.Given("У RTU три недоступных основных переключателя", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 12
 testRunner.When("Первая инициализация этого RTU", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 13
 testRunner.Then("В дереве у RTU портов - 1", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Xunit.FactAttribute(DisplayName="Начальная инициализация Veex RTU без Otau")]
        [Xunit.TraitAttribute("FeatureTitle", "VeexRtuFirstInitialization")]
        [Xunit.TraitAttribute("Description", "Начальная инициализация Veex RTU без Otau")]
        public virtual void НачальнаяИнициализацияVeexRTUБезOtau()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Начальная инициализация Veex RTU без Otau", null, ((string[])(null)));
#line 15
this.ScenarioInitialize(scenarioInfo);
            this.ScenarioStart();
#line 2
this.FeatureBackground();
#line 16
 testRunner.Given("У RTU нет основного переключателя", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 17
 testRunner.When("Первая инициализация этого RTU", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 18
 testRunner.Then("В дереве у RTU портов - 1", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Xunit.FactAttribute(DisplayName="Начальная инициализация Veex RTU с несколькими основными Otau")]
        [Xunit.TraitAttribute("FeatureTitle", "VeexRtuFirstInitialization")]
        [Xunit.TraitAttribute("Description", "Начальная инициализация Veex RTU с несколькими основными Otau")]
        public virtual void НачальнаяИнициализацияVeexRTUСНесколькимиОсновнымиOtau()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Начальная инициализация Veex RTU с несколькими основными Otau", null, ((string[])(null)));
#line 20
this.ScenarioInitialize(scenarioInfo);
            this.ScenarioStart();
#line 2
this.FeatureBackground();
#line 21
 testRunner.Given("У RTU три основных переключателя и только один с восьмью портами доступен", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 22
 testRunner.When("Первая инициализация этого RTU", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 23
 testRunner.Then("В дереве у RTU портов - 8", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Xunit.FactAttribute(DisplayName="Начальная инициализация Veex RTU с основным и доп Otau")]
        [Xunit.TraitAttribute("FeatureTitle", "VeexRtuFirstInitialization")]
        [Xunit.TraitAttribute("Description", "Начальная инициализация Veex RTU с основным и доп Otau")]
        public virtual void НачальнаяИнициализацияVeexRTUСОсновнымИДопOtau()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Начальная инициализация Veex RTU с основным и доп Otau", null, ((string[])(null)));
#line 25
this.ScenarioInitialize(scenarioInfo);
            this.ScenarioStart();
#line 2
this.FeatureBackground();
#line 26
 testRunner.Given("У RTU основной на 32 и доп на 16 портов переключатели", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 27
 testRunner.When("Первая инициализация этого RTU", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 28
 testRunner.Then("В дереве у RTU портов - 32", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.4.0.0")]
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
        public class FixtureData : System.IDisposable
        {
            
            public FixtureData()
            {
                VeexRtuFirstInitializationFeature.FeatureSetup();
            }
            
            void System.IDisposable.Dispose()
            {
                VeexRtuFirstInitializationFeature.FeatureTearDown();
            }
        }
    }
}
#pragma warning restore
#endregion
