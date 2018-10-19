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
namespace Graph.Tests.Node
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.1.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public partial class NodeRemovedFeature : Xunit.IClassFixture<NodeRemovedFeature.FixtureData>, System.IDisposable
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "NodeRemoved.feature"
#line hidden
        
        public NodeRemovedFeature()
        {
            this.TestInitialize();
        }
        
        public static void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "NodeRemoved", null, ProgrammingLanguage.CSharp, ((string[])(null)));
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
 testRunner.Given("Существует узел", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
        }
        
        public virtual void SetFixture(NodeRemovedFeature.FixtureData fixtureData)
        {
        }
        
        void System.IDisposable.Dispose()
        {
            this.ScenarioTearDown();
        }
        
        [Xunit.FactAttribute(DisplayName="Удаление одинокого узла")]
        [Xunit.TraitAttribute("FeatureTitle", "NodeRemoved")]
        [Xunit.TraitAttribute("Description", "Удаление одинокого узла")]
        public virtual void УдалениеОдинокогоУзла()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Удаление одинокого узла", ((string[])(null)));
#line 6
this.ScenarioSetup(scenarioInfo);
#line 3
this.FeatureBackground();
#line 7
 testRunner.When("Пользователь кликает удалить узел", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 8
 testRunner.Then("Узел удаляется", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Xunit.FactAttribute(DisplayName="Удаление узла с подключенным отрезком")]
        [Xunit.TraitAttribute("FeatureTitle", "NodeRemoved")]
        [Xunit.TraitAttribute("Description", "Удаление узла с подключенным отрезком")]
        public virtual void УдалениеУзлаСПодключеннымОтрезком()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Удаление узла с подключенным отрезком", ((string[])(null)));
#line 10
this.ScenarioSetup(scenarioInfo);
#line 3
this.FeatureBackground();
#line 11
 testRunner.Given("К данному узлу присоединен отрезок", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 12
 testRunner.When("Пользователь кликает удалить узел", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 13
 testRunner.Then("Отрезки связанные с исходным узлом удаляются", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 14
 testRunner.Then("Узел удаляется", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Xunit.FactAttribute(DisplayName="Удаление узла с точкой привязки на подключенном отрезке")]
        [Xunit.TraitAttribute("FeatureTitle", "NodeRemoved")]
        [Xunit.TraitAttribute("Description", "Удаление узла с точкой привязки на подключенном отрезке")]
        public virtual void УдалениеУзлаСТочкойПривязкиНаПодключенномОтрезке()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Удаление узла с точкой привязки на подключенном отрезке", ((string[])(null)));
#line 16
this.ScenarioSetup(scenarioInfo);
#line 3
this.FeatureBackground();
#line 17
 testRunner.Given("К данному узлу присоединен отрезок", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 18
 testRunner.And("На отрезкок добавлена точка привязки", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 19
 testRunner.When("Пользователь кликает удалить узел", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 20
 testRunner.Then("Удаляется весь отрезок вплоть до соседнего узла-не точки", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 21
 testRunner.And("Удаляется точка привязки", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 22
 testRunner.Then("Узел удаляется", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Xunit.FactAttribute(DisplayName="Попытка удаления узла последнего для трассы")]
        [Xunit.TraitAttribute("FeatureTitle", "NodeRemoved")]
        [Xunit.TraitAttribute("Description", "Попытка удаления узла последнего для трассы")]
        public virtual void ПопыткаУдаленияУзлаПоследнегоДляТрассы()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Попытка удаления узла последнего для трассы", ((string[])(null)));
#line 24
this.ScenarioSetup(scenarioInfo);
#line 3
this.FeatureBackground();
#line 25
 testRunner.Given("Задана трасса", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 26
 testRunner.Given("Данный узел последний в трассе", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 27
 testRunner.When("Пользователь кликает удалить узел", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 28
 testRunner.Then("Удаление не происходит", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Xunit.FactAttribute(DisplayName="Удаление узла НЕ последнего для трассы")]
        [Xunit.TraitAttribute("FeatureTitle", "NodeRemoved")]
        [Xunit.TraitAttribute("Description", "Удаление узла НЕ последнего для трассы")]
        public virtual void УдалениеУзлаНЕПоследнегоДляТрассы()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Удаление узла НЕ последнего для трассы", ((string[])(null)));
#line 30
this.ScenarioSetup(scenarioInfo);
#line 3
this.FeatureBackground();
#line 31
 testRunner.Given("Задана трасса", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 32
 testRunner.Given("Данный узел НЕ последний в трассе", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 33
 testRunner.When("Пользователь кликает удалить узел", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 34
 testRunner.Then("Создается отрезок между соседними с данным узлами", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 35
 testRunner.Then("Корректируются списки узлов и оборудования трассы", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 36
 testRunner.Then("Отрезки связанные с исходным узлом удаляются", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 37
 testRunner.Then("Узел удаляется", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Xunit.FactAttribute(DisplayName="Удаление НЕ последнего узла рядом с точкой привязки")]
        [Xunit.TraitAttribute("FeatureTitle", "NodeRemoved")]
        [Xunit.TraitAttribute("Description", "Удаление НЕ последнего узла рядом с точкой привязки")]
        public virtual void УдалениеНЕПоследнегоУзлаРядомСТочкойПривязки()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Удаление НЕ последнего узла рядом с точкой привязки", ((string[])(null)));
#line 39
this.ScenarioSetup(scenarioInfo);
#line 3
this.FeatureBackground();
#line 40
 testRunner.Given("Задана трасса", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 41
 testRunner.Given("Данный узел НЕ последний в трассе", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 42
 testRunner.And("На трассу добавлена точка привязки", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 43
 testRunner.When("Пользователь кликает удалить узел", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 44
 testRunner.Then("Создается отрезок между RTU и точкой привязки", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 45
 testRunner.Then("Корректируются списки узлов и оборудования трассы", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 46
 testRunner.Then("Отрезки связанные с исходным узлом удаляются", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 47
 testRunner.Then("Узел удаляется", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Xunit.FactAttribute(DisplayName="Попытка удаления узла из трассы с базовой")]
        [Xunit.TraitAttribute("FeatureTitle", "NodeRemoved")]
        [Xunit.TraitAttribute("Description", "Попытка удаления узла из трассы с базовой")]
        public virtual void ПопыткаУдаленияУзлаИзТрассыСБазовой()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Попытка удаления узла из трассы с базовой", ((string[])(null)));
#line 49
this.ScenarioSetup(scenarioInfo);
#line 3
this.FeatureBackground();
#line 50
 testRunner.Given("Задана трасса", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 51
 testRunner.Given("Данный узел НЕ последний в трассе", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 52
 testRunner.Given("Для трассы задана базовая", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 53
 testRunner.When("Пользователь кликает удалить узел", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 54
 testRunner.Then("Удаление не происходит", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.1.0.0")]
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
        public class FixtureData : System.IDisposable
        {
            
            public FixtureData()
            {
                NodeRemovedFeature.FeatureSetup();
            }
            
            void System.IDisposable.Dispose()
            {
                NodeRemovedFeature.FeatureTearDown();
            }
        }
    }
}
#pragma warning restore
#endregion