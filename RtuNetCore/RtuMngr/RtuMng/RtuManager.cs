using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsNetCore;
using Microsoft.Extensions.Logging;

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

    // RtuService set its token on execution start
    // System uses it to stop whole RTU daemon
    public CancellationToken RtuServiceCancellationToken;
    
    // Plus create new Source into _rtuManagerCts then create new Token
    // every time before starting monitoring cycle or doing Out-of-turn or Client's measurement
    // during these long operations check array of 2 tokens (system and local)
    // 
    // When User requests ApplyMonitoringSettings or StopMonitoring 
    //  we call Cancel on _rtuManagerCts 
    // or Recovery asks Initialization
    private CancellationTokenSource? _rtuManagerCts;

    // public readonly ConcurrentQueue<object> ShouldSendHeartbeat = new();

    // private bool _wasMonitoringOn;
    public bool IsMonitoringOn;

    public string Version { get; set; } = null!;

    private readonly object _lastSuccessfulMeasTimestampLocker = new object();
    private DateTime _lastSuccessfulMeasTimestamp;
    public DateTime LastSuccessfulMeasTimestamp
    {
        get { lock (_lastSuccessfulMeasTimestampLocker) { return _lastSuccessfulMeasTimestamp; } }
        set { lock (_lastSuccessfulMeasTimestampLocker) { _lastSuccessfulMeasTimestamp = value; } }
    }

    private readonly object _currentStepLocker = new object();
    private CurrentMonitoringStepDto _currentStep;
    public CurrentMonitoringStepDto CurrentStep
    {
        get { lock (_currentStepLocker) { return _currentStep; } }
        set { lock (_currentStepLocker) { _currentStep = value; } }
    }

    private readonly object _initializationResultLocker = new object();
    private InitializationResult? _initializationResult;
    public InitializationResult? InitializationResult
    {
        get { lock (_initializationResultLocker) { return _initializationResult; } }
        set { lock (_initializationResultLocker) { _initializationResult = value; } }
    }

    public RtuManager(IWritableConfig<RtuConfig> config,
        ILogger<RtuManager> logger,
        InterOpWrapper interOpWrapper, OtdrManager otdrManager,
        IServiceProvider serviceProvider)
    {
        _currentStep = new CurrentMonitoringStepDto() { Step = MonitoringCurrentStep.Idle };
        _config = config;
        _logger = logger;
        _serialPortManager = new SerialPortManager();
        _serialPortManager.Initialize(_config.Value.Charon, logger);
        _interOpWrapper = interOpWrapper;
        _otdrManager = otdrManager;
        _serviceProvider = serviceProvider;
    }
}