﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WcfConnections.C2DWcfServiceReference {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="C2DWcfServiceReference.IWcfServiceForClient")]
    public interface IWcfServiceForClient {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWcfServiceForClient/RegisterClient", ReplyAction="http://tempuri.org/IWcfServiceForClient/RegisterClientResponse")]
        void RegisterClient(Dto.RegisterClientDto dto);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWcfServiceForClient/UnRegisterClient", ReplyAction="http://tempuri.org/IWcfServiceForClient/UnRegisterClientResponse")]
        void UnRegisterClient(Dto.UnRegisterClientDto dto);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWcfServiceForClient/CheckServerConnection", ReplyAction="http://tempuri.org/IWcfServiceForClient/CheckServerConnectionResponse")]
        bool CheckServerConnection(Dto.CheckServerConnectionDto dto);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWcfServiceForClient/CheckRtuConnection", ReplyAction="http://tempuri.org/IWcfServiceForClient/CheckRtuConnectionResponse")]
        bool CheckRtuConnection(Dto.CheckRtuConnectionDto rtuAddress);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWcfServiceForClient/InitializeRtu", ReplyAction="http://tempuri.org/IWcfServiceForClient/InitializeRtuResponse")]
        bool InitializeRtu(Dto.InitializeRtuDto rtu);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWcfServiceForClient/StartMonitoring", ReplyAction="http://tempuri.org/IWcfServiceForClient/StartMonitoringResponse")]
        bool StartMonitoring(Dto.StartMonitoringDto dto);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWcfServiceForClient/StopMonitoring", ReplyAction="http://tempuri.org/IWcfServiceForClient/StopMonitoringResponse")]
        bool StopMonitoring(Dto.StopMonitoringDto dto);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWcfServiceForClient/ApplyMonitoringSettings", ReplyAction="http://tempuri.org/IWcfServiceForClient/ApplyMonitoringSettingsResponse")]
        bool ApplyMonitoringSettings(Dto.ApplyMonitoringSettingsDto settings);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWcfServiceForClient/AssignBaseRef", ReplyAction="http://tempuri.org/IWcfServiceForClient/AssignBaseRefResponse")]
        bool AssignBaseRef(Dto.AssignBaseRefDto baseRef);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IWcfServiceForClientChannel : WcfConnections.C2DWcfServiceReference.IWcfServiceForClient, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class WcfServiceForClientClient : System.ServiceModel.ClientBase<WcfConnections.C2DWcfServiceReference.IWcfServiceForClient>, WcfConnections.C2DWcfServiceReference.IWcfServiceForClient {
        
        public WcfServiceForClientClient() {
        }
        
        public WcfServiceForClientClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public WcfServiceForClientClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public WcfServiceForClientClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public WcfServiceForClientClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public void RegisterClient(Dto.RegisterClientDto dto) {
            base.Channel.RegisterClient(dto);
        }
        
        public void UnRegisterClient(Dto.UnRegisterClientDto dto) {
            base.Channel.UnRegisterClient(dto);
        }
        
        public bool CheckServerConnection(Dto.CheckServerConnectionDto dto) {
            return base.Channel.CheckServerConnection(dto);
        }
        
        public bool CheckRtuConnection(Dto.CheckRtuConnectionDto rtuAddress) {
            return base.Channel.CheckRtuConnection(rtuAddress);
        }
        
        public bool InitializeRtu(Dto.InitializeRtuDto rtu) {
            return base.Channel.InitializeRtu(rtu);
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
    }
}
