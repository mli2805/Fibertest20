using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public class RtuLinuxPollsterThread
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly GlobalState _globalState;
        private readonly Model _writeModel;
        private readonly RtuStationsRepository _rtuStationsRepository;

        private List<Rtu> _makLinuxRtus;
        private TimeSpan _gap;

        public RtuLinuxPollsterThread(IniFile iniFile, IMyLog logFile, 
            GlobalState globalState, Model writeModel, RtuStationsRepository rtuStationsRepository)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _globalState = globalState;
            _writeModel = writeModel;
            _rtuStationsRepository = rtuStationsRepository;
        }

        public void Start()
        {
            var thread = new Thread(Init) { IsBackground = true };
            thread.Start();
        }

        // ReSharper disable once FunctionNeverReturns
        private async void Init()
        {
            _gap = TimeSpan.FromSeconds(_iniFile.Read(IniSection.General, IniKey.MakLinuxPollsterGap, 1));
            _logFile.AppendLine("MAK with Linux pollster started in thread");

            while (true)
            {
                if (!_globalState.IsDatacenterInDbOptimizationMode)
                {
                    await Tick();
                }
                Thread.Sleep(_gap);
            }
        }

        private async Task Tick()
        {
            _makLinuxRtus = _writeModel.Rtus.Where(r => r.MainChannel.Port == (int)TcpPorts.RtuListenToHttp && r.IsInitialized).ToList();
            var stations = await _rtuStationsRepository.GetAllRtuStations();

            //TODO get current state of RTU?
        }

    }
}
