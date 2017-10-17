﻿using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.RtuWcfServiceInterface
{
    [ServiceContract]
    public interface IRtuWcfServiceBackward
    {
        [OperationContract(IsOneWay = true)]
        void EndInitialize(RtuInitializedDto dto);

        [OperationContract(IsOneWay = true)]
        void EndStartMonitoring(bool result);
    }

    public class Handler<T>
    {
        private readonly Queue<TaskCompletionSource<T>> _handler = new Queue<TaskCompletionSource<T>>();
        public void AddHandler(TaskCompletionSource<T> handler) => _handler.Enqueue(handler);
        public void End(T result) => _handler.Dequeue().TrySetResult(result);
    }

    public class RtuWcfServiceBackward : IRtuWcfServiceBackward
    {
        public Handler<RtuInitializedDto> HandlerForInitializeRtu { get; } = new Handler<RtuInitializedDto>();
        public void EndInitialize(RtuInitializedDto dto) => HandlerForInitializeRtu.End(dto);

        public Handler<bool> HandlerForStartMonitoring { get; } = new Handler<bool>();
        public void EndStartMonitoring(bool result) => HandlerForStartMonitoring.End(result);
    }

    [ServiceContract(CallbackContract = typeof(IRtuWcfServiceBackward))]
    public interface IRtuWcfService
    {
        [OperationContract]
        void BeginInitialize(InitializeRtuDto dto);

        [OperationContract]
        void BeginStartMonitoring(StartMonitoringDto dto);


        [OperationContract]
        bool StopMonitoring(StopMonitoringDto dto);

        [OperationContract]
        bool ApplyMonitoringSettings(ApplyMonitoringSettingsDto settings);

        [OperationContract]
        bool AssignBaseRef(AssignBaseRefDto baseRef);


        // for Client
        [OperationContract]
        bool ToggleToPort(OtauPortDto port);


        // for WatchDog
        [OperationContract]
        bool CheckLastSuccessfullMeasTime();
    }

    public static class RtuWcfServiceExtension
    {
        public static Task<RtuInitializedDto> InitializeAsync(
                 this IRtuWcfService rtuWcfService, RtuWcfServiceBackward backwardService, InitializeRtuDto dto)
        {
            var src = new TaskCompletionSource<RtuInitializedDto>();
            backwardService.HandlerForInitializeRtu.AddHandler(src);
            rtuWcfService.BeginInitialize(dto);
            return src.Task;
        }
        public static Task<bool> StartMonitoringAsync(
                 this IRtuWcfService rtuWcfService, RtuWcfServiceBackward backwardService, StartMonitoringDto dto)
        {
            var src = new TaskCompletionSource<bool>();
            backwardService.HandlerForStartMonitoring.AddHandler(src);
            rtuWcfService.BeginStartMonitoring(dto);
            return src.Task;
        }
    }
}