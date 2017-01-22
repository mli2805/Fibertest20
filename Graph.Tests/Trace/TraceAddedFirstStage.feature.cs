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
namespace Graph.Tests.Trace
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.1.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public partial class TraceAddedFirstStageFeature : Xunit.IClassFixture<TraceAddedFirstStageFeature.FixtureData>, System.IDisposable
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "TraceAddedFirstStage.feature"
#line hidden
        
        public TraceAddedFirstStageFeature()
        {
            this.TestInitialize();
        }
        
        public static void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "TraceAddedFirstStage", null, ProgrammingLanguage.CSharp, ((string[])(null)));
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
 testRunner.Given("Существуют РТУ оборудование узлы и отрезки между ними", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
        }
        
        public virtual void SetFixture(TraceAddedFirstStageFeature.FixtureData fixtureData)
        {
        }
        
        void System.IDisposable.Dispose()
        {
            this.ScenarioTearDown();
        }
        
        [Xunit.FactAttribute(DisplayName="В последнем узле нет оборудования")]
        [Xunit.TraitAttribute("FeatureTitle", "TraceAddedFirstStage")]
        [Xunit.TraitAttribute("Description", "В последнем узле нет оборудования")]
        public virtual void ВПоследнемУзлеНетОборудования()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("В последнем узле нет оборудования", ((string[])(null)));
#line 6
this.ScenarioSetup(scenarioInfo);
#line 3
this.FeatureBackground();
#line 7
 testRunner.Given("И кликнул определить трассу на узле где нет оборудования", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 8
 testRunner.Then("Сообщение Last node of trace must contain some equipment", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Xunit.FactAttribute(DisplayName="Добавление трассы где нет пути")]
        [Xunit.TraitAttribute("FeatureTitle", "TraceAddedFirstStage")]
        [Xunit.TraitAttribute("Description", "Добавление трассы где нет пути")]
        public virtual void ДобавлениеТрассыГдеНетПути()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Добавление трассы где нет пути", ((string[])(null)));
#line 10
this.ScenarioSetup(scenarioInfo);
#line 3
this.FeatureBackground();
#line 11
 testRunner.Given("Между выбираемыми узлами нет пути", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 12
 testRunner.Given("Но пользователь выбрал узел где есть оборудование и кликнул определить трассу", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 13
 testRunner.Then("Сообщение Path couldn\'t be found", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Xunit.FactAttribute(DisplayName="Пользователя не устроил предложенный маршрут")]
        [Xunit.TraitAttribute("FeatureTitle", "TraceAddedFirstStage")]
        [Xunit.TraitAttribute("Description", "Пользователя не устроил предложенный маршрут")]
        public virtual void ПользователяНеУстроилПредложенныйМаршрут()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Пользователя не устроил предложенный маршрут", ((string[])(null)));
#line 15
this.ScenarioSetup(scenarioInfo);
#line 3
this.FeatureBackground();
#line 16
 testRunner.Given("На вопрос: \"Accept the path?\" пользователь ответил: \"Cancel\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 17
 testRunner.Given("Хотя кликнул определить трассу на узле с оборудованием и путь между узлами сущест" +
                    "вует", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 18
 testRunner.Then("Новая трасса не сохраняется", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.1.0.0")]
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
        public class FixtureData : System.IDisposable
        {
            
            public FixtureData()
            {
                TraceAddedFirstStageFeature.FeatureSetup();
            }
            
            void System.IDisposable.Dispose()
            {
                TraceAddedFirstStageFeature.FeatureTearDown();
            }
        }
    }
}
#pragma warning restore
#endregion
