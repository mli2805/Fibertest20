﻿using System.Collections.Concurrent;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsNet6;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Iit.Fibertest.RtuMngr;

public partial class RtuManager
{
    private readonly IWritableConfig<RtuConfig> _config;
    private readonly ILogger<RtuManager> _logger;
    private readonly SerialPortManager _serialPortManager;
    private readonly InterOpWrapper _interOpWrapper;
    private readonly OtdrManager _otdrManager;
    private readonly IServiceProvider _serviceProvider;

    private Charon _mainCharon = null!;
    private int _measurementNumber;
    private TimeSpan _preciseMakeTimespan;
    private TimeSpan _preciseSaveTimespan;
    private TimeSpan _fastSaveTimespan;

    private TreeOfAcceptableMeasParams? _treeOfAcceptableMeasParams;
    public CancellationToken RtuServiceCancellationToken;
    private CancellationTokenSource? _rtuManagerCts;
    private MonitoringQueue _monitoringQueue;
    public readonly ConcurrentQueue<object> ShouldSendHeartbeat = new ConcurrentQueue<object>();

    private CancellationTokenSource? _cancellationTokenSource;
    private bool _wasMonitoringOn;
    public bool IsMonitoringOn;

    private readonly object _lastSuccessfulMeasTimestampLocker = new object();
    private DateTime _lastSuccessfulMeasTimestamp;
    public DateTime LastSuccessfulMeasTimestamp
    {
        get { lock (_lastSuccessfulMeasTimestampLocker) { return _lastSuccessfulMeasTimestamp; } }
        set { lock (_lastSuccessfulMeasTimestampLocker) { _lastSuccessfulMeasTimestamp = value; } }
    }

    private readonly object _isRtuInitializedLocker = new object();
    private bool _isRtuInitialized;
    public bool IsRtuInitialized
    {
        get { lock (_isRtuInitializedLocker) { return _isRtuInitialized; } }
        set { lock (_isRtuInitializedLocker) { _isRtuInitialized = value; } }
    }

    private readonly object _currentStepLocker = new object();
    private CurrentMonitoringStepDto _currentStep;
    public CurrentMonitoringStepDto CurrentStep
    {
        get { lock (_currentStepLocker) { return _currentStep; } }
        set { lock (_currentStepLocker) { _currentStep = value; } }
    }



    public RtuManager(IWritableConfig<RtuConfig> config,
        ILogger<RtuManager> logger, MonitoringQueue monitoringQueue,
        InterOpWrapper interOpWrapper, OtdrManager otdrManager, 
        IServiceProvider serviceProvider)
    {
        IsRtuInitialized = false;
        _config = config;
        _logger = logger;
        _monitoringQueue = monitoringQueue;
        _serialPortManager = new SerialPortManager();
        _serialPortManager.Initialize(_config.Value.Charon, logger);
        _interOpWrapper = interOpWrapper;
        _otdrManager = otdrManager;
        _serviceProvider = serviceProvider;
    }

    private async Task SaveMoniResult(MonitoringResultEf entity)
    {
        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<MonitoringResultsRepository>();
        await repo.Add(entity);
    }

    private static readonly JsonSerializerSettings JsonSerializerSettings =
        new() { TypeNameHandling = TypeNameHandling.All };
    // ClientMeasurementResultDto, BopStateChangedDto, ??? rtu accidents how?
    private async Task SaveEvent(object obj)
    {
        var json = JsonConvert.SerializeObject(obj, JsonSerializerSettings);
        _logger.Info(Logs.RtuService, json);

        using var scope = _serviceProvider.CreateScope();
        var eventsRepository = scope.ServiceProvider.GetRequiredService<EventsRepository>();
        await eventsRepository.Add(json);
    }
}