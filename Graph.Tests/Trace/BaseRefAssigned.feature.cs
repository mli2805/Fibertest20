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
    public partial class BaseRefAssignedFeature : Xunit.IClassFixture<BaseRefAssignedFeature.FixtureData>, System.IDisposable
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "BaseRefAssigned.feature"
#line hidden
        
        public BaseRefAssignedFeature()
        {
            this.TestInitialize();
        }
        
        public static void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "BaseRefAssigned", null, ProgrammingLanguage.CSharp, ((string[])(null)));
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
        
        public virtual void SetFixture(BaseRefAssignedFeature.FixtureData fixtureData)
        {
        }
        
        void System.IDisposable.Dispose()
        {
            this.ScenarioTearDown();
        }
        
        [Xunit.FactAttribute(DisplayName="Задание базовой рефлектограммы")]
        [Xunit.TraitAttribute("FeatureTitle", "BaseRefAssigned")]
        [Xunit.TraitAttribute("Description", "Задание базовой рефлектограммы")]
        public virtual void ЗаданиеБазовойРефлектограммы()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Задание базовой рефлектограммы", ((string[])(null)));
#line 3
this.ScenarioSetup(scenarioInfo);
#line 4
 testRunner.Given("Существует трасса", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 5
 testRunner.And("Открыта форма для задания базовых", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 6
 testRunner.When("Пользователь жмет сохранить", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 7
 testRunner.Then("Изменения сохраняются", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.1.0.0")]
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
        public class FixtureData : System.IDisposable
        {
            
            public FixtureData()
            {
                BaseRefAssignedFeature.FeatureSetup();
            }
            
            void System.IDisposable.Dispose()
            {
                BaseRefAssignedFeature.FeatureTearDown();
            }
        }
    }
}
#pragma warning restore
#endregion
