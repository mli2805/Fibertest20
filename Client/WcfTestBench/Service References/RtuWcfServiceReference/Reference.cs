﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WcfTestBench.RtuWcfServiceReference {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="RtuWcfServiceReference.IRtuWcfService")]
    public interface IRtuWcfService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRtuWcfService/ShakeHandsWithWatchdog", ReplyAction="http://tempuri.org/IRtuWcfService/ShakeHandsWithWatchdogResponse")]
        string ShakeHandsWithWatchdog(string hello);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRtuWcfService/ShakeHandsWithWatchdog", ReplyAction="http://tempuri.org/IRtuWcfService/ShakeHandsWithWatchdogResponse")]
        System.Threading.Tasks.Task<string> ShakeHandsWithWatchdogAsync(string hello);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRtuWcfService/Initialize", ReplyAction="http://tempuri.org/IRtuWcfService/InitializeResponse")]
        bool Initialize(Dto.InitializeRtuDto rtu);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRtuWcfService/Initialize", ReplyAction="http://tempuri.org/IRtuWcfService/InitializeResponse")]
        System.Threading.Tasks.Task<bool> InitializeAsync(Dto.InitializeRtuDto rtu);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRtuWcfService/StartMonitoring", ReplyAction="http://tempuri.org/IRtuWcfService/StartMonitoringResponse")]
        void StartMonitoring();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRtuWcfService/StartMonitoring", ReplyAction="http://tempuri.org/IRtuWcfService/StartMonitoringResponse")]
        System.Threading.Tasks.Task StartMonitoringAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRtuWcfService/StopMonitoring", ReplyAction="http://tempuri.org/IRtuWcfService/StopMonitoringResponse")]
        void StopMonitoring();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRtuWcfService/StopMonitoring", ReplyAction="http://tempuri.org/IRtuWcfService/StopMonitoringResponse")]
        System.Threading.Tasks.Task StopMonitoringAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRtuWcfService/ApplyMonitoringSettings", ReplyAction="http://tempuri.org/IRtuWcfService/ApplyMonitoringSettingsResponse")]
        bool ApplyMonitoringSettings(Dto.ApplyMonitoringSettingsDto settings);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRtuWcfService/ApplyMonitoringSettings", ReplyAction="http://tempuri.org/IRtuWcfService/ApplyMonitoringSettingsResponse")]
        System.Threading.Tasks.Task<bool> ApplyMonitoringSettingsAsync(Dto.ApplyMonitoringSettingsDto settings);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRtuWcfService/AssignBaseRef", ReplyAction="http://tempuri.org/IRtuWcfService/AssignBaseRefResponse")]
        bool AssignBaseRef(Dto.AssignBaseRefDto baseRef);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRtuWcfService/AssignBaseRef", ReplyAction="http://tempuri.org/IRtuWcfService/AssignBaseRefResponse")]
        System.Threading.Tasks.Task<bool> AssignBaseRefAsync(Dto.AssignBaseRefDto baseRef);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRtuWcfService/ToggleToPort", ReplyAction="http://tempuri.org/IRtuWcfService/ToggleToPortResponse")]
        bool ToggleToPort(Dto.OtauPortDto port);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRtuWcfService/ToggleToPort", ReplyAction="http://tempuri.org/IRtuWcfService/ToggleToPortResponse")]
        System.Threading.Tasks.Task<bool> ToggleToPortAsync(Dto.OtauPortDto port);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IRtuWcfServiceChannel : WcfTestBench.RtuWcfServiceReference.IRtuWcfService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class RtuWcfServiceClient : System.ServiceModel.ClientBase<WcfTestBench.RtuWcfServiceReference.IRtuWcfService>, WcfTestBench.RtuWcfServiceReference.IRtuWcfService {
        
        public RtuWcfServiceClient() {
        }
        
        public RtuWcfServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public RtuWcfServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public RtuWcfServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public RtuWcfServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public string ShakeHandsWithWatchdog(string hello) {
            return base.Channel.ShakeHandsWithWatchdog(hello);
        }
        
        public System.Threading.Tasks.Task<string> ShakeHandsWithWatchdogAsync(string hello) {
            return base.Channel.ShakeHandsWithWatchdogAsync(hello);
        }
        
        public bool Initialize(Dto.InitializeRtuDto rtu) {
            return base.Channel.Initialize(rtu);
        }
        
        public System.Threading.Tasks.Task<bool> InitializeAsync(Dto.InitializeRtuDto rtu) {
            return base.Channel.InitializeAsync(rtu);
        }
        
        public void StartMonitoring() {
            base.Channel.StartMonitoring();
        }
        
        public System.Threading.Tasks.Task StartMonitoringAsync() {
            return base.Channel.StartMonitoringAsync();
        }
        
        public void StopMonitoring() {
            base.Channel.StopMonitoring();
        }
        
        public System.Threading.Tasks.Task StopMonitoringAsync() {
            return base.Channel.StopMonitoringAsync();
        }
        
        public bool ApplyMonitoringSettings(Dto.ApplyMonitoringSettingsDto settings) {
            return base.Channel.ApplyMonitoringSettings(settings);
        }
        
        public System.Threading.Tasks.Task<bool> ApplyMonitoringSettingsAsync(Dto.ApplyMonitoringSettingsDto settings) {
            return base.Channel.ApplyMonitoringSettingsAsync(settings);
        }
        
        public bool AssignBaseRef(Dto.AssignBaseRefDto baseRef) {
            return base.Channel.AssignBaseRef(baseRef);
        }
        
        public System.Threading.Tasks.Task<bool> AssignBaseRefAsync(Dto.AssignBaseRefDto baseRef) {
            return base.Channel.AssignBaseRefAsync(baseRef);
        }
        
        public bool ToggleToPort(Dto.OtauPortDto port) {
            return base.Channel.ToggleToPort(port);
        }
        
        public System.Threading.Tasks.Task<bool> ToggleToPortAsync(Dto.OtauPortDto port) {
            return base.Channel.ToggleToPortAsync(port);
        }
    }
}
