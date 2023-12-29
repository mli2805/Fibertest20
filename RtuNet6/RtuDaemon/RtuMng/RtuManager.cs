using System.Collections.Concurrent;
using Iit.Fibertest.CharonLib;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsNet6;

namespace Iit.Fibertest.RtuDaemon;

public partial class RtuManager
{
    private readonly IWritableConfig<RtuConfig> _config;
    private readonly ILogger<RtuManager> _logger;
    private readonly SerialPortManager _serialPortManager;
    private readonly InterOpWrapper _interOpWrapper;
    private readonly OtdrManager _otdrManager;
    private readonly MessageStorage _messageStorage;

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

    public RtuManager(IWritableConfig<RtuConfig> config,
        ILogger<RtuManager> logger, MonitoringQueue monitoringQueue,
        InterOpWrapper interOpWrapper, OtdrManager otdrManager, MessageStorage messageStorage)
    {
        IsRtuInitialized = false;
        _config = config;
        _logger = logger;
        _monitoringQueue = monitoringQueue;
        _serialPortManager = new SerialPortManager();
        _serialPortManager.Initialize(_config.Value.Charon, logger);
        _interOpWrapper = interOpWrapper;
        _otdrManager = otdrManager;
        _messageStorage = messageStorage;
    }
}