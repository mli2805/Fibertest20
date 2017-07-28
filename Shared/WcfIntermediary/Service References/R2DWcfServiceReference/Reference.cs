﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WcfIntermediary.R2DWcfServiceReference {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="R2DWcfServiceReference.IWcfServiceForRtu")]
    public interface IWcfServiceForRtu {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWcfServiceForRtu/ProcessRtuInitialized", ReplyAction="http://tempuri.org/IWcfServiceForRtu/ProcessRtuInitializedResponse")]
        bool ProcessRtuInitialized(Dto.RtuInitializedDto result);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWcfServiceForRtu/ProcessRtuInitialized", ReplyAction="http://tempuri.org/IWcfServiceForRtu/ProcessRtuInitializedResponse")]
        System.Threading.Tasks.Task<bool> ProcessRtuInitializedAsync(Dto.RtuInitializedDto result);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWcfServiceForRtu/ConfirmStartMonitoring", ReplyAction="http://tempuri.org/IWcfServiceForRtu/ConfirmStartMonitoringResponse")]
        bool ConfirmStartMonitoring(Dto.MonitoringStartedDto result);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWcfServiceForRtu/ConfirmStartMonitoring", ReplyAction="http://tempuri.org/IWcfServiceForRtu/ConfirmStartMonitoringResponse")]
        System.Threading.Tasks.Task<bool> ConfirmStartMonitoringAsync(Dto.MonitoringStartedDto result);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWcfServiceForRtu/ConfirmStopMonitoring", ReplyAction="http://tempuri.org/IWcfServiceForRtu/ConfirmStopMonitoringResponse")]
        bool ConfirmStopMonitoring(Dto.MonitoringStoppedDto result);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWcfServiceForRtu/ConfirmStopMonitoring", ReplyAction="http://tempuri.org/IWcfServiceForRtu/ConfirmStopMonitoringResponse")]
        System.Threading.Tasks.Task<bool> ConfirmStopMonitoringAsync(Dto.MonitoringStoppedDto result);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWcfServiceForRtu/ProcessMonitoringResult", ReplyAction="http://tempuri.org/IWcfServiceForRtu/ProcessMonitoringResultResponse")]
        bool ProcessMonitoringResult(Dto.MonitoringResult result);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWcfServiceForRtu/ProcessMonitoringResult", ReplyAction="http://tempuri.org/IWcfServiceForRtu/ProcessMonitoringResultResponse")]
        System.Threading.Tasks.Task<bool> ProcessMonitoringResultAsync(Dto.MonitoringResult result);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWcfServiceForRtu/ConfirmMonitoringSettingsApplied", ReplyAction="http://tempuri.org/IWcfServiceForRtu/ConfirmMonitoringSettingsAppliedResponse")]
        bool ConfirmMonitoringSettingsApplied(Dto.MonitoringSettingsAppliedDto result);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWcfServiceForRtu/ConfirmMonitoringSettingsApplied", ReplyAction="http://tempuri.org/IWcfServiceForRtu/ConfirmMonitoringSettingsAppliedResponse")]
        System.Threading.Tasks.Task<bool> ConfirmMonitoringSettingsAppliedAsync(Dto.MonitoringSettingsAppliedDto result);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWcfServiceForRtu/ConfirmBaseRefAssigned", ReplyAction="http://tempuri.org/IWcfServiceForRtu/ConfirmBaseRefAssignedResponse")]
        bool ConfirmBaseRefAssigned(Dto.BaseRefAssignedDto result);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWcfServiceForRtu/ConfirmBaseRefAssigned", ReplyAction="http://tempuri.org/IWcfServiceForRtu/ConfirmBaseRefAssignedResponse")]
        System.Threading.Tasks.Task<bool> ConfirmBaseRefAssignedAsync(Dto.BaseRefAssignedDto result);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IWcfServiceForRtuChannel : WcfIntermediary.R2DWcfServiceReference.IWcfServiceForRtu, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class WcfServiceForRtuClient : System.ServiceModel.ClientBase<WcfIntermediary.R2DWcfServiceReference.IWcfServiceForRtu>, WcfIntermediary.R2DWcfServiceReference.IWcfServiceForRtu {
        
        public WcfServiceForRtuClient() {
        }
        
        public WcfServiceForRtuClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public WcfServiceForRtuClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public WcfServiceForRtuClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public WcfServiceForRtuClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public bool ProcessRtuInitialized(Dto.RtuInitializedDto result) {
            return base.Channel.ProcessRtuInitialized(result);
        }
        
        public System.Threading.Tasks.Task<bool> ProcessRtuInitializedAsync(Dto.RtuInitializedDto result) {
            return base.Channel.ProcessRtuInitializedAsync(result);
        }
        
        public bool ConfirmStartMonitoring(Dto.MonitoringStartedDto result) {
            return base.Channel.ConfirmStartMonitoring(result);
        }
        
        public System.Threading.Tasks.Task<bool> ConfirmStartMonitoringAsync(Dto.MonitoringStartedDto result) {
            return base.Channel.ConfirmStartMonitoringAsync(result);
        }
        
        public bool ConfirmStopMonitoring(Dto.MonitoringStoppedDto result) {
            return base.Channel.ConfirmStopMonitoring(result);
        }
        
        public System.Threading.Tasks.Task<bool> ConfirmStopMonitoringAsync(Dto.MonitoringStoppedDto result) {
            return base.Channel.ConfirmStopMonitoringAsync(result);
        }
        
        public bool ProcessMonitoringResult(Dto.MonitoringResult result) {
            return base.Channel.ProcessMonitoringResult(result);
        }
        
        public System.Threading.Tasks.Task<bool> ProcessMonitoringResultAsync(Dto.MonitoringResult result) {
            return base.Channel.ProcessMonitoringResultAsync(result);
        }
        
        public bool ConfirmMonitoringSettingsApplied(Dto.MonitoringSettingsAppliedDto result) {
            return base.Channel.ConfirmMonitoringSettingsApplied(result);
        }
        
        public System.Threading.Tasks.Task<bool> ConfirmMonitoringSettingsAppliedAsync(Dto.MonitoringSettingsAppliedDto result) {
            return base.Channel.ConfirmMonitoringSettingsAppliedAsync(result);
        }
        
        public bool ConfirmBaseRefAssigned(Dto.BaseRefAssignedDto result) {
            return base.Channel.ConfirmBaseRefAssigned(result);
        }
        
        public System.Threading.Tasks.Task<bool> ConfirmBaseRefAssignedAsync(Dto.BaseRefAssignedDto result) {
            return base.Channel.ConfirmBaseRefAssignedAsync(result);
        }
    }
}
