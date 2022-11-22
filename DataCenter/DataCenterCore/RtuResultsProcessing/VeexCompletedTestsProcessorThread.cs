using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public class VeexCompletedTestsProcessorThread
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly GlobalState _globalState;
        private readonly VeexCompletedTestProcessor _veexCompletedTestProcessor;
        public readonly ConcurrentQueue<Tuple<CompletedTest, Rtu>> CompletedTests = new ConcurrentQueue<Tuple<CompletedTest, Rtu>>();

        private TimeSpan _gap;

        public VeexCompletedTestsProcessorThread(IniFile iniFile, IMyLog logFile, GlobalState globalState,
            VeexCompletedTestProcessor veexCompletedTestProcessor)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _globalState = globalState;
            _veexCompletedTestProcessor = veexCompletedTestProcessor;
        }

        public void Start()
        {
            var thread = new Thread(Init) { IsBackground = true };
            thread.Start();
        }

        // ReSharper disable once FunctionNeverReturns
        private async void Init()
        {
            _gap = TimeSpan.FromSeconds(_iniFile.Read(IniSection.General, IniKey.CheckCompletedTestsEvery, 1));
            _logFile.AppendLine("VeEX completed tests processor started in thread");

            while (true)
            {
                if (!_globalState.IsDatacenterInDbOptimizationMode)
                {
                    await Tick();
                }
            }
        }

        public async Task Tick()
        {
            if (!CompletedTests.TryDequeue(out var next))
            {
                // _logFile.AppendLine("Queue is empty");
                Thread.Sleep(_gap);
                return;
            }
            await _veexCompletedTestProcessor.ProcessOneCompletedTest(next.Item1, next.Item2);
        }
    }
}
