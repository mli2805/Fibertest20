using System;
using System.Threading.Tasks;
using Iit.Fibertest.DirectCharonLibrary;

namespace Iit.Fibertest.IitOtdrLibrary
{
    public partial class OtdrManager
    {
        public async Task<bool> MeasureWithBaseAsync(byte[] buffer, Charon activeChild, IProgress<int> progress)
        {
            var result = false;

            // allocate memory inside c++ library
            // put there base sor data
            // return pointer to that data, than you can say c++ code to use this data
            var baseSorData = IitOtdr.SetSorData(buffer);
            var isSuccess = await TaskEx.Run(()=> IitOtdr.SetMeasurementParametersFromSor(ref baseSorData));
            if (isSuccess)
            {
                IitOtdr.ForceLmaxNs(IitOtdr.ConvertLmaxOwtToNs(buffer));
                result = await MeasureAsync(activeChild, progress);
            }

            // free memory where was base sor data
            IitOtdr.FreeSorDataMemory(baseSorData);
            return result;
        }
        public async Task DoManualMeasurementAsync(bool shouldForceLmax, Charon activeChild, IProgress<int> progress)
        {
            if (shouldForceLmax)
                IitOtdr.ForceLmaxNs(IitOtdr.ConvertLmaxKmToNs());

            await MeasureAsync(activeChild, progress);

        }
        private async Task<bool> MeasureAsync(Charon activeChild, IProgress<int> progress)
        {
            _rtuLogger.AppendLine("Measurement begin.");
            lock (_lockObj)
            {
                _isMeasurementCanceled = false;
            }

            var isSuccess = await TaskEx.Run(() => IitOtdr.PrepareMeasurement(true));
            if (!isSuccess)
            {
                _rtuLogger.AppendLine("Prepare measurement error!");
                return false;
            }

            activeChild?.ShowMessageMeasurementPort();

            var result = await MeasureLoopAsync(progress);

            _rtuLogger.AppendLine("Measurement end.");

            activeChild?.ShowOnDisplayMessageReady();

            return result;
        }



        private async Task<bool> MeasureLoopAsync(IProgress<int> progress)
        {
            try
            {
                bool hasMoreSteps;
                do
                {
                    lock (_lockObj)
                    {
                        if (_isMeasurementCanceled)
                        {
                            IitOtdr.StopMeasurement(true);
                            _rtuLogger.AppendLine("Measurement interrupted.");
                            break;
                        }
                    }

                    progress?.Report(1);
//                    hasMoreSteps = IitOtdr.DoMeasurementStep(ref _sorData);
                    hasMoreSteps = await TaskEx.Run(()=> IitOtdr.DoMeasurementStep(ref _sorData));
                }
                while (hasMoreSteps);
            }
            catch (Exception e)
            {
                _rtuLogger.AppendLine(e.Message);
                return false;
            }
            return true;
        }
    }
}
