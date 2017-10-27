using System;
using System.Threading;
using System.Threading.Tasks;
using Iit.Fibertest.DirectCharonLibrary;

namespace Iit.Fibertest.IitOtdrLibrary
{
    public partial class OtdrManager
    {
        public async Task<bool> MeasureWithBaseAsync(byte[] buffer, Charon activeChild, IProgress<int> progress, CancellationToken token)
        {
            var result = false;

            // allocate memory inside c++ library
            // put there base sor data
            // return pointer to that data, than you can say c++ code to use this data
            var baseSorData = IitOtdr.SetSorData(buffer);

            try
            {
                var isSuccess = await TaskEx.Run(() => IitOtdr.SetMeasurementParametersFromSor(ref baseSorData), token);
                if (isSuccess)
                {
                    IitOtdr.ForceLmaxNs(IitOtdr.ConvertLmaxOwtToNs(buffer));
                    result = await MeasureAsync(activeChild, progress, token);
                }
            }
            catch (OperationCanceledException)
            {
                _rtuLogger.AppendLine("Measurement interrupted.");
                progress?.Report(-1);
                result = true;
            }
            catch (Exception e)
            {
                _rtuLogger.AppendLine($"MeasureWithBaseAsync {e.Message}");
                result = false;
            }
            finally
            {
                // free memory where was base sor data
                IitOtdr.FreeSorDataMemory(baseSorData);
            }
            return result;
        }
        public async Task DoManualMeasurementAsync(bool shouldForceLmax, Charon activeChild, IProgress<int> progress, CancellationToken token)
        {
            if (shouldForceLmax)
                IitOtdr.ForceLmaxNs(IitOtdr.ConvertLmaxKmToNs());

            await MeasureAsync(activeChild, progress, token);

        }
        private async Task<bool> MeasureAsync(Charon activeChild, IProgress<int> progress, CancellationToken token)
        {
            _rtuLogger.AppendLine("Measurement begin.");
//            lock (_lockObj)
//            {
//                _isMeasurementCanceled = false;
//            }

            var isSuccess = await TaskEx.Run(() => IitOtdr.PrepareMeasurement(true), token);
            if (!isSuccess)
            {
                _rtuLogger.AppendLine("Prepare measurement error!");
                return false;
            }

            activeChild?.ShowMessageMeasurementPort();

            var result = await MeasureLoopAsync(progress, token);

            _rtuLogger.AppendLine("Measurement end.");

            activeChild?.ShowOnDisplayMessageReady();

            return result;
        }



        private async Task<bool> MeasureLoopAsync(IProgress<int> progress, CancellationToken token)
        {
            try
            {
                bool hasMoreSteps;
                int count = 0;
                do
                {
//                    lock (_lockObj)
//                    {
//                        if (_isMeasurementCanceled)
//                        {
//                            IitOtdr.StopMeasurement(true);
//                            _rtuLogger.AppendLine("Measurement interrupted.");
//                            break;
//                        }
//                    }

                    count++;
                    progress?.Report(count);
                    hasMoreSteps = await TaskEx.Run(()=> IitOtdr.DoMeasurementStep(ref _sorData), token);
                }
                while (hasMoreSteps);
            }
            catch (OperationCanceledException)
            {
                _rtuLogger.AppendLine("Measurement interrupted.");
                progress?.Report(-1);
                return false;
            }
            catch (Exception e)
            {
                _rtuLogger.AppendLine("MeasureLoopAsync: " + e.Message);
                return false;
            }
            return true;
        }
    }
}
