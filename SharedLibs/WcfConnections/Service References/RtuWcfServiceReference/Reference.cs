﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WcfConnections.RtuWcfServiceReference {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="RtuWcfServiceReference.IRtuWcfService")]
    public interface IRtuWcfService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRtuWcfService/Initialize", ReplyAction="http://tempuri.org/IRtuWcfService/InitializeResponse")]
        bool Initialize(Dto.InitializeRtuDto rtu);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRtuWcfService/StartMonitoring", ReplyAction="http://tempuri.org/IRtuWcfService/StartMonitoringResponse")]
        bool StartMonitoring(Dto.StartMonitoringDto dto);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRtuWcfService/StopMonitoring", ReplyAction="http://tempuri.org/IRtuWcfService/StopMonitoringResponse")]
        bool StopMonitoring(Dto.StopMonitoringDto dto);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRtuWcfService/ApplyMonitoringSettings", ReplyAction="http://tempuri.org/IRtuWcfService/ApplyMonitoringSettingsResponse")]
        bool ApplyMonitoringSettings(Dto.ApplyMonitoringSettingsDto settings);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRtuWcfService/AssignBaseRef", ReplyAction="http://tempuri.org/IRtuWcfService/AssignBaseRefResponse")]
        bool AssignBaseRef(Dto.AssignBaseRefDto baseRef);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRtuWcfService/ToggleToPort", ReplyAction="http://tempuri.org/IRtuWcfService/ToggleToPortResponse")]
        bool ToggleToPort(Dto.OtauPortDto port);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRtuWcfService/CheckLastSuccessfullMeasTime", ReplyAction="http://tempuri.org/IRtuWcfService/CheckLastSuccessfullMeasTimeResponse")]
        bool CheckLastSuccessfullMeasTime();
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IRtuWcfServiceChannel : WcfConnections.RtuWcfServiceReference.IRtuWcfService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class RtuWcfServiceClient : System.ServiceModel.ClientBase<WcfConnections.RtuWcfServiceReference.IRtuWcfService>, WcfConnections.RtuWcfServiceReference.IRtuWcfService {
        
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
        
        public bool Initialize(Dto.InitializeRtuDto rtu) {
            return base.Channel.Initialize(rtu);
        }
        
        public bool StartMonitoring(Dto.StartMonitoringDto dto) {
            return base.Channel.StartMonitoring(dto);
        }
        
        public bool StopMonitoring(Dto.StopMonitoringDto dto) {
            return base.Channel.StopMonitoring(dto);
        }
        
        public bool ApplyMonitoringSettings(Dto.ApplyMonitoringSettingsDto settings) {
            return base.Channel.ApplyMonitoringSettings(settings);
        }
        
        public bool AssignBaseRef(Dto.AssignBaseRefDto baseRef) {
            return base.Channel.AssignBaseRef(baseRef);
        }
        
        public bool ToggleToPort(Dto.OtauPortDto port) {
            return base.Channel.ToggleToPort(port);
        }
        
        public bool CheckLastSuccessfullMeasTime() {
            return base.Channel.CheckLastSuccessfullMeasTime();
        }
    }
}
