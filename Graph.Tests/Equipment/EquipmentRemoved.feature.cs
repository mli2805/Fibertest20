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
    public partial class EquipmentRemovedFeature : Xunit.IClassFixture<EquipmentRemovedFeature.FixtureData>, System.IDisposable
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "EquipmentRemoved.feature"
#line hidden
        
        public EquipmentRemovedFeature()
        {
            this.TestInitialize();
        }
        
        public static void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "EquipmentRemoved", null, ProgrammingLanguage.CSharp, ((string[])(null)));
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
 testRunner.Given("Существует узел A с оборудованием A1", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 5
 testRunner.Given("Существует узел B с оборудованием B1", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 6
 testRunner.Given("Существуют RTU и волокна", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 7
 testRunner.Given("Существует трасса c оборудованием А1 в середине и B1 в конце", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
        }
        
        public virtual void SetFixture(EquipmentRemovedFeature.FixtureData fixtureData)
        {
        }
        
        void System.IDisposable.Dispose()
        {
            this.ScenarioTearDown();
        }
        
        [Xunit.FactAttribute(DisplayName="Удаление оборудования не последнего для любой трассы")]
        [Xunit.TraitAttribute("FeatureTitle", "EquipmentRemoved")]
        [Xunit.TraitAttribute("Description", "Удаление оборудования не последнего для любой трассы")]
        public virtual void УдалениеОборудованияНеПоследнегоДляЛюбойТрассы()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Удаление оборудования не последнего для любой трассы", ((string[])(null)));
#line 9
this.ScenarioSetup(scenarioInfo);
#line 3
this.FeatureBackground();
#line 10
 testRunner.Given("Открыта форма для редактирования узла где оборудование А1", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 11
 testRunner.Then("Пункт Удалить доступен для данного оборудования", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 12
 testRunner.When("Пользователь нажимает удалить оборудование", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 13
 testRunner.Then("Оборудование удаляется из трассы", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 14
 testRunner.Then("Оборудование удаляется", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Xunit.FactAttribute(DisplayName="Попытка удаления оборудования из трассы с базовой")]
        [Xunit.TraitAttribute("FeatureTitle", "EquipmentRemoved")]
        [Xunit.TraitAttribute("Description", "Попытка удаления оборудования из трассы с базовой")]
        public virtual void ПопыткаУдаленияОборудованияИзТрассыСБазовой()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Попытка удаления оборудования из трассы с базовой", ((string[])(null)));
#line 16
this.ScenarioSetup(scenarioInfo);
#line 3
this.FeatureBackground();
#line 17
 testRunner.Given("Для этой трассы задана базовая", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 18
 testRunner.Given("Открыта форма для редактирования узла где оборудование А1", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 19
 testRunner.Then("Пункт Удалить недоступен для оборудования A1", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Xunit.FactAttribute(DisplayName="Попытка удаления оборудования последнего для трассы")]
        [Xunit.TraitAttribute("FeatureTitle", "EquipmentRemoved")]
        [Xunit.TraitAttribute("Description", "Попытка удаления оборудования последнего для трассы")]
        public virtual void ПопыткаУдаленияОборудованияПоследнегоДляТрассы()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Попытка удаления оборудования последнего для трассы", ((string[])(null)));
#line 21
this.ScenarioSetup(scenarioInfo);
#line 3
this.FeatureBackground();
#line 22
 testRunner.Given("Открыта форма для редактирования узла где оборудование B1", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 23
 testRunner.Then("Пункт Удалить недоступен для оборудования B1", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.1.0.0")]
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
        public class FixtureData : System.IDisposable
        {
            
            public FixtureData()
            {
                EquipmentRemovedFeature.FeatureSetup();
            }
            
            void System.IDisposable.Dispose()
            {
                EquipmentRemovedFeature.FeatureTearDown();
            }
        }
    }
}
#pragma warning restore
#endregion
