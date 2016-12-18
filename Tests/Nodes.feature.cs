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
namespace Tests
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.1.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public partial class NodesFeature : Xunit.IClassFixture<NodesFeature.FixtureData>, System.IDisposable
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "Nodes.feature"
#line hidden
        
        public NodesFeature()
        {
            this.TestInitialize();
        }
        
        public static void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Nodes", "\tIn order to know where geographically placed my equipment\r\n\tAs a root user\r\n\tI w" +
                    "ant to be able to create, update and delete nodes", ProgrammingLanguage.CSharp, ((string[])(null)));
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
        
        public virtual void SetFixture(NodesFeature.FixtureData fixtureData)
        {
        }
        
        void System.IDisposable.Dispose()
        {
            this.ScenarioTearDown();
        }
        
        [Xunit.FactAttribute(DisplayName="Create node")]
        [Xunit.TraitAttribute("FeatureTitle", "Nodes")]
        [Xunit.TraitAttribute("Description", "Create node")]
        public virtual void CreateNode()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Create node", ((string[])(null)));
#line 6
this.ScenarioSetup(scenarioInfo);
#line 7
 testRunner.Given("I call CreateNode(1.23, 1.23)", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 8
 testRunner.When("I call GetGraph()", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 9
 testRunner.Then("the return value should be", "   { \r\n      \"Nodes\": [\r\n             { \r\n\t\t\t\t\"Id\" : 0,\r\n\t\t\t\t\"Title\" : null,\r\n   " +
                    "             \"Coordinates\": {\r\n\t\t\t\t\t\"Latitude\" : 1.23,\r\n\t\t\t\t\t\"Longitude\" : 1.23\r" +
                    "\n\t\t\t\t}\r\n             }\r\n      ]\r\n   }", ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Xunit.FactAttribute(DisplayName="Update node")]
        [Xunit.TraitAttribute("FeatureTitle", "Nodes")]
        [Xunit.TraitAttribute("Description", "Update node")]
        public virtual void UpdateNode()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Update node", ((string[])(null)));
#line 25
this.ScenarioSetup(scenarioInfo);
#line 26
 testRunner.Given("I call CreateNode(1.23, 1.23)", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
#line 27
 testRunner.And("I call UpdateNode", "{\r\n\t\t\t\t\"Id\" : 0,\r\n\t\t\t\t\"Title\" : \"Hello world!\",\r\n                \"Coordinates\": {" +
                    "\r\n\t\t\t\t\t\"Latitude\" : 1.23,\r\n\t\t\t\t\t\"Longitude\" : 1.23\r\n\t\t\t\t}\r\n}", ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 38
 testRunner.When("I call GetGraph()", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 39
 testRunner.Then("the return value should be", "   { \r\n      \"Nodes\": [\r\n             { \r\n\t\t\t\t\"Id\" : 0,\r\n\t\t\t\t\"Title\" : \"Hello wor" +
                    "ld!\",\r\n                \"Coordinates\": {\r\n\t\t\t\t\t\"Latitude\" : 1.23,\r\n\t\t\t\t\t\"Longitud" +
                    "e\" : 1.23\r\n\t\t\t\t}\r\n             }\r\n      ]\r\n   }", ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.1.0.0")]
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
        public class FixtureData : System.IDisposable
        {
            
            public FixtureData()
            {
                NodesFeature.FeatureSetup();
            }
            
            void System.IDisposable.Dispose()
            {
                NodesFeature.FeatureTearDown();
            }
        }
    }
}
#pragma warning restore
#endregion
