using System.Threading.Tasks;
using Iit.Fibertest.D2RtuVeexLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public class VeexCompletedTestProcessor
    {
        private readonly IMyLog _logFile;
        private readonly Model _writeModel;
        private readonly D2RtuVeexLayer3 _d2RtuVeexLayer3;

        public VeexCompletedTestProcessor(IMyLog logFile, Model writeModel, D2RtuVeexLayer3 d2RtuVeexLayer3)
        {
            _logFile = logFile;
            _writeModel = writeModel;
            _d2RtuVeexLayer3 = d2RtuVeexLayer3;
        }

        public async Task ProcessOneCompletedTest(CompletedTest completedTest)
        {

        }
    }
}