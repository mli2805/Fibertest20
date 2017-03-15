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
    public partial class EquipmentAddedFeature : Xunit.IClassFixture<EquipmentAddedFeature.FixtureData>, System.IDisposable
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "EquipmentAdded.feature"
#line hidden
        
        public EquipmentAddedFeature()
        {
            this.TestInitialize();
        }
        
        public static void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "EquipmentAdded", null, ProgrammingLanguage.CSharp, ((string[])(null)));
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
 testRunner.Given("Дан узел с оборудованием", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 5
 testRunner.Given("Еще есть РТУ другие узлы и волокна", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 6
 testRunner.Given("Одна трасса заканчивается в данном узле", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 7
 testRunner.Given("Одна трасса проходит через данный узел и использует оборудование", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 8
 testRunner.Given("Одна трасса проходит через данный узел но не использует оборудование", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
        }
        
        public virtual void SetFixture(EquipmentAddedFeature.FixtureData fixtureData)
        {
        }
        
        void System.IDisposable.Dispose()
        {
            this.ScenarioTearDown();
        }
        
        [Xunit.FactAttribute(DisplayName="Добавление в узел трассы но не в трассу")]
        [Xunit.TraitAttribute("FeatureTitle", "EquipmentAdded")]
        [Xunit.TraitAttribute("Description", "Добавление в узел трассы но не в трассу")]
        public virtual void ДобавлениеВУзелТрассыНоНеВТрассу()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Добавление в узел трассы но не в трассу", ((string[])(null)));
#line 10
this.ScenarioSetup(scenarioInfo);
#line 3
this.FeatureBackground();
#line 11
 testRunner.Then("Пользователь не отмечает ни одну трассу для включения оборудования", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 12
 testRunner.Then("Пользователь вводит парамы оборудования и жмет Сохранить", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 13
 testRunner.Then("В окне Добавить оборудование", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 14
 testRunner.Then("В узле создается новое оборудование", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 15
 testRunner.Then("Но в трассы оно не входит", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Xunit.FactAttribute(DisplayName="Добавление в трассы")]
        [Xunit.TraitAttribute("FeatureTitle", "EquipmentAdded")]
        [Xunit.TraitAttribute("Description", "Добавление в трассы")]
        public virtual void ДобавлениеВТрассы()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Добавление в трассы", ((string[])(null)));
#line 17
this.ScenarioSetup(scenarioInfo);
#line 3
this.FeatureBackground();
#line 18
 testRunner.Then("Пользователь отмечает все трассы для включения оборудования", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 19
 testRunner.Then("Пользователь вводит парамы оборудования и жмет Сохранить", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 20
 testRunner.Then("В окне Добавить оборудование", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 21
 testRunner.Then("В узле создается новое оборудование", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 22
 testRunner.Then("Новое оборудование входит во все трассы а старое ни в одну", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Xunit.FactAttribute(DisplayName="Добавление в трассу с заданной базовой недоступно")]
        [Xunit.TraitAttribute("FeatureTitle", "EquipmentAdded")]
        [Xunit.TraitAttribute("Description", "Добавление в трассу с заданной базовой недоступно")]
        public virtual void ДобавлениеВТрассуСЗаданнойБазовойНедоступно()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Добавление в трассу с заданной базовой недоступно", ((string[])(null)));
#line 25
this.ScenarioSetup(scenarioInfo);
#line 3
this.FeatureBackground();
#line 26
 testRunner.Given("Для одной из трасс задана базовая", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 27
 testRunner.Then("На форме выбора трасс эта трасса недоступна для выбора остальные доступны", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Xunit.FactAttribute(DisplayName="Отказ от добавления оборудования")]
        [Xunit.TraitAttribute("FeatureTitle", "EquipmentAdded")]
        [Xunit.TraitAttribute("Description", "Отказ от добавления оборудования")]
        public virtual void ОтказОтДобавленияОборудования()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Отказ от добавления оборудования", ((string[])(null)));
#line 29
this.ScenarioSetup(scenarioInfo);
#line 3
this.FeatureBackground();
#line 30
 testRunner.Then("Пользователь отмечает все трассы для включения оборудования", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 31
 testRunner.Then("Пользователь вводит парамы оборудования и жмет Отмена", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 32
 testRunner.Then("В окне Добавить оборудование", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 33
 testRunner.Then("В узле НЕ создается новое оборудование", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.1.0.0")]
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
        public class FixtureData : System.IDisposable
        {
            
            public FixtureData()
            {
                EquipmentAddedFeature.FeatureSetup();
            }
            
            void System.IDisposable.Dispose()
            {
                EquipmentAddedFeature.FeatureTearDown();
            }
        }
    }
}
#pragma warning restore
#endregion
