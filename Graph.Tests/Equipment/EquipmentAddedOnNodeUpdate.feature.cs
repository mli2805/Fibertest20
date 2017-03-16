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
    public partial class EquipmentAddedOnNodeUpdateFeature : Xunit.IClassFixture<EquipmentAddedOnNodeUpdateFeature.FixtureData>, System.IDisposable
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "EquipmentAddedOnNodeUpdate.feature"
#line hidden
        
        public EquipmentAddedOnNodeUpdateFeature()
        {
            this.TestInitialize();
        }
        
        public static void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "EquipmentAddedOnNodeUpdate", "\tДобавление оборудования в узел на форме изменения узла \r\n\t(Еще можно добавлять в" +
                    " узел на карте из контекстного меню узла,\r\n\tтестирование различных вариантов при" +
                    " добавлении сосредоточено там)", ProgrammingLanguage.CSharp, ((string[])(null)));
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
#line 6
#line 7
 testRunner.Given("Через узел проходят три трассы", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 8
 testRunner.Given("Пользователь открывает форму редактирования узла", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
        }
        
        public virtual void SetFixture(EquipmentAddedOnNodeUpdateFeature.FixtureData fixtureData)
        {
        }
        
        void System.IDisposable.Dispose()
        {
            this.ScenarioTearDown();
        }
        
        [Xunit.FactAttribute(DisplayName="Добавление оборудования")]
        [Xunit.TraitAttribute("FeatureTitle", "EquipmentAddedOnNodeUpdate")]
        [Xunit.TraitAttribute("Description", "Добавление оборудования")]
        public virtual void ДобавлениеОборудования()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Добавление оборудования", ((string[])(null)));
#line 10
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 11
 testRunner.When("Пользователь жмет добавить оборудование вводит парамы и сохраняет", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 12
 testRunner.Then("Новое оборудование сохраняется и появляется на форме редактирования узла", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Xunit.FactAttribute(DisplayName="Отказ от добавления оборудования")]
        [Xunit.TraitAttribute("FeatureTitle", "EquipmentAddedOnNodeUpdate")]
        [Xunit.TraitAttribute("Description", "Отказ от добавления оборудования")]
        public virtual void ОтказОтДобавленияОборудования()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Отказ от добавления оборудования", ((string[])(null)));
#line 14
this.ScenarioSetup(scenarioInfo);
#line 6
this.FeatureBackground();
#line 15
 testRunner.When("Пользователь жмет добавить оборудование вводит парамы и резко отказывается от сох" +
                    "ранения", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 16
 testRunner.Then("Новое оборудование НЕ сохраняется и НЕ появляется на форме редактирования узла", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.1.0.0")]
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
        public class FixtureData : System.IDisposable
        {
            
            public FixtureData()
            {
                EquipmentAddedOnNodeUpdateFeature.FeatureSetup();
            }
            
            void System.IDisposable.Dispose()
            {
                EquipmentAddedOnNodeUpdateFeature.FeatureTearDown();
            }
        }
    }
}
#pragma warning restore
#endregion
