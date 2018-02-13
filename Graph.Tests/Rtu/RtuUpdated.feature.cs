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
    public partial class RtuUpdatedFeature : Xunit.IClassFixture<RtuUpdatedFeature.FixtureData>, System.IDisposable
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "RtuUpdated.feature"
#line hidden
        
        public RtuUpdatedFeature()
        {
            this.TestInitialize();
        }
        
        public static void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "RtuUpdated", null, ProgrammingLanguage.CSharp, ((string[])(null)));
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
 testRunner.Given("Ранее был создан RTU с именем blah-blah", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 5
 testRunner.Given("Добавлен RTU с именем название рту", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
        }
        
        public virtual void SetFixture(RtuUpdatedFeature.FixtureData fixtureData)
        {
        }
        
        void System.IDisposable.Dispose()
        {
            this.ScenarioTearDown();
        }
        
        [Xunit.FactAttribute(DisplayName="Сохранение без изменений")]
        [Xunit.TraitAttribute("FeatureTitle", "RtuUpdated")]
        [Xunit.TraitAttribute("Description", "Сохранение без изменений")]
        public virtual void СохранениеБезИзменений()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Сохранение без изменений", ((string[])(null)));
#line 7
this.ScenarioSetup(scenarioInfo);
#line 3
this.FeatureBackground();
#line 8
 testRunner.When("Пользователь открыл окно редактирования первого RTU и ничего не изменив нажал Сох" +
                    "ранить", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 9
 testRunner.Then("Команд не подается", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Xunit.FactAttribute(DisplayName="Сохранение изменений")]
        [Xunit.TraitAttribute("FeatureTitle", "RtuUpdated")]
        [Xunit.TraitAttribute("Description", "Сохранение изменений")]
        public virtual void СохранениеИзменений()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Сохранение изменений", ((string[])(null)));
#line 11
this.ScenarioSetup(scenarioInfo);
#line 3
this.FeatureBackground();
#line 12
 testRunner.Given("Пользователь ввел название нового RTU node-node", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 13
 testRunner.Then("Сохраняется название RTU node-node", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Xunit.FactAttribute(DisplayName="Сохранение изменений комментария")]
        [Xunit.TraitAttribute("FeatureTitle", "RtuUpdated")]
        [Xunit.TraitAttribute("Description", "Сохранение изменений комментария")]
        public virtual void СохранениеИзмененийКомментария()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Сохранение изменений комментария", ((string[])(null)));
#line 15
this.ScenarioSetup(scenarioInfo);
#line 3
this.FeatureBackground();
#line 16
 testRunner.Given("Пользователь ввел комментарий к RTU some comment", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 17
 testRunner.Then("Сохраняется комментарий RTU some comment", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Xunit.FactAttribute(DisplayName="Сохранение с существующим именем RTU")]
        [Xunit.TraitAttribute("FeatureTitle", "RtuUpdated")]
        [Xunit.TraitAttribute("Description", "Сохранение с существующим именем RTU")]
        public virtual void СохранениеССуществующимИменемRTU()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Сохранение с существующим именем RTU", ((string[])(null)));
#line 19
this.ScenarioSetup(scenarioInfo);
#line 3
this.FeatureBackground();
#line 20
 testRunner.Given("Пользователь открыл окно нового RTU и ввел название существующего blah-blah", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 21
 testRunner.Then("Кнопка Сохранить заблокирована поле Название подсвечено", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Xunit.FactAttribute(DisplayName="Отказ от изменения RTU")]
        [Xunit.TraitAttribute("FeatureTitle", "RtuUpdated")]
        [Xunit.TraitAttribute("Description", "Отказ от изменения RTU")]
        public virtual void ОтказОтИзмененияRTU()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Отказ от изменения RTU", ((string[])(null)));
#line 23
this.ScenarioSetup(scenarioInfo);
#line 3
this.FeatureBackground();
#line 24
 testRunner.When("Пользователь открыл окно редактирования RTU и что-то изменив нажал Отменить", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 25
 testRunner.Then("Команд не подается", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Xunit.FactAttribute(DisplayName="Пустое название")]
        [Xunit.TraitAttribute("FeatureTitle", "RtuUpdated")]
        [Xunit.TraitAttribute("Description", "Пустое название")]
        public virtual void ПустоеНазвание()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Пустое название", ((string[])(null)));
#line 27
this.ScenarioSetup(scenarioInfo);
#line 3
this.FeatureBackground();
#line 28
 testRunner.When("Пользователь открыл окно редактирования нового RTU", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 29
 testRunner.When("Пользователь очищает поле Название", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 30
 testRunner.Then("Кнопка Сохранить становится недоступна", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.1.0.0")]
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
        public class FixtureData : System.IDisposable
        {
            
            public FixtureData()
            {
                RtuUpdatedFeature.FeatureSetup();
            }
            
            void System.IDisposable.Dispose()
            {
                RtuUpdatedFeature.FeatureTearDown();
            }
        }
    }
}
#pragma warning restore
#endregion
