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
namespace Graph.Tests.Equipment
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.1.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public partial class EquipmentRemovedWithLoopFeature : Xunit.IClassFixture<EquipmentRemovedWithLoopFeature.FixtureData>, System.IDisposable
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "EquipmentRemovedWithLoop.feature"
#line hidden
        
        public EquipmentRemovedWithLoopFeature()
        {
            this.TestInitialize();
        }
        
        public static void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "EquipmentRemovedWithLoop", null, ProgrammingLanguage.CSharp, ((string[])(null)));
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
 testRunner.Given("Трасса использует муфту А1 дважды", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
        }
        
        public virtual void SetFixture(EquipmentRemovedWithLoopFeature.FixtureData fixtureData)
        {
        }
        
        void System.IDisposable.Dispose()
        {
            this.ScenarioTearDown();
        }
        
        [Xunit.FactAttribute(DisplayName="Удаление оборудования входящего в трассу дважды но не последнего")]
        [Xunit.TraitAttribute("FeatureTitle", "EquipmentRemovedWithLoop")]
        [Xunit.TraitAttribute("Description", "Удаление оборудования входящего в трассу дважды но не последнего")]
        public virtual void УдалениеОборудованияВходящегоВТрассуДваждыНоНеПоследнего()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Удаление оборудования входящего в трассу дважды но не последнего", ((string[])(null)));
#line 6
this.ScenarioSetup(scenarioInfo);
#line 3
this.FeatureBackground();
#line 7
 testRunner.Given("Открыта форма для редактирования узла с муфтой А1", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 8
 testRunner.When("Пользователь удаляет оборудование", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 9
 testRunner.Then("Оборудование удаляется из обоих мест в трассе и в целом из графа", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Xunit.FactAttribute(DisplayName="Удаление двойного оборудования из одного шага")]
        [Xunit.TraitAttribute("FeatureTitle", "EquipmentRemovedWithLoop")]
        [Xunit.TraitAttribute("Description", "Удаление двойного оборудования из одного шага")]
        public virtual void УдалениеДвойногоОборудованияИзОдногоШага()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Удаление двойного оборудования из одного шага", ((string[])(null)));
#line 11
this.ScenarioSetup(scenarioInfo);
#line 3
this.FeatureBackground();
#line 12
 testRunner.Given("Открыта форма ориентиров", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 13
 testRunner.And("Две строки содержат муфту", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 14
 testRunner.When("Пользователь исключает муфту из трассы на первом проходе", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 15
 testRunner.Then("На первом проходе муфты нет на обратном есть", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.1.0.0")]
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
        public class FixtureData : System.IDisposable
        {
            
            public FixtureData()
            {
                EquipmentRemovedWithLoopFeature.FeatureSetup();
            }
            
            void System.IDisposable.Dispose()
            {
                EquipmentRemovedWithLoopFeature.FeatureTearDown();
            }
        }
    }
}
#pragma warning restore
#endregion