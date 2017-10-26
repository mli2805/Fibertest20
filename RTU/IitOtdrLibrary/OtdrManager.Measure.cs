﻿using System;
using System.Threading.Tasks;
using Iit.Fibertest.DirectCharonLibrary;
using Microsoft;
using Optixsoft.SorExaminer.OtdrDataFormat;

namespace Iit.Fibertest.IitOtdrLibrary
{
    public partial class OtdrManager
    {
        public bool MeasureWithBase(byte[] buffer, Charon activeChild)
        {
            var result = false;

            // allocate memory inside c++ library
            // put there base sor data
            // return pointer to that data, than you can say c++ code to use this data
            var baseSorData = IitOtdr.SetSorData(buffer);
            if (IitOtdr.SetMeasurementParametersFromSor(ref baseSorData))
            {
                IitOtdr.ForceLmaxNs(IitOtdr.ConvertLmaxOwtToNs(buffer));
                result = Measure(activeChild);
            }

            // free memory where was base sor data
            IitOtdr.FreeSorDataMemory(baseSorData);
            return result;
        }

        public async Task<bool> MeasureWithBaseAsync(byte[] buffer, Charon activeChild, IProgress<int> progress)
        {
            var result = false;

            // allocate memory inside c++ library
            // put there base sor data
            // return pointer to that data, than you can say c++ code to use this data
            var baseSorData = IitOtdr.SetSorData(buffer);
            if (IitOtdr.SetMeasurementParametersFromSor(ref baseSorData))
            {
                IitOtdr.ForceLmaxNs(IitOtdr.ConvertLmaxOwtToNs(buffer));
                result = await MeasureAsync(activeChild, progress);
            }

            // free memory where was base sor data
            IitOtdr.FreeSorDataMemory(baseSorData);
            return result;
        }

        public bool DoManualMeasurement(bool shouldForceLmax, Charon activeChild)
        {
            if (shouldForceLmax)
                IitOtdr.ForceLmaxNs(IitOtdr.ConvertLmaxKmToNs());

            return Measure(activeChild);
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


            if (!IitOtdr.PrepareMeasurement(true))
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
                    hasMoreSteps = IitOtdr.DoMeasurementStep(ref _sorData);
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

        private readonly object _lockObj = new object();
        private bool _isMeasurementCanceled;
        private IntPtr _sorData = IntPtr.Zero;

        /// <summary>
        /// after Measure() use GetLastSorData() to obtain measurement result
        /// </summary>
        /// <returns></returns>
        private bool Measure(Charon activeChild)
        {
            _rtuLogger.AppendLine("Measurement begin.");
            lock (_lockObj)
            {
                _isMeasurementCanceled = false;
            }


            if (!IitOtdr.PrepareMeasurement(true))
            {
                _rtuLogger.AppendLine("Prepare measurement error!");
                return false;
            }

            activeChild?.ShowMessageMeasurementPort();

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

                    hasMoreSteps = IitOtdr.DoMeasurementStep(ref _sorData);
                }
                while (hasMoreSteps);
            }
            catch (Exception e)
            {
                _rtuLogger.AppendLine(e.Message);
                return false;
            }

            _rtuLogger.AppendLine("Measurement end.");

            activeChild?.ShowOnDisplayMessageReady();

            return true;
        }

        public void InterruptMeasurement()
        {
            lock (_lockObj)
            {
                _isMeasurementCanceled = true;
            }
        }

        public byte[] GetLastSorDataBuffer()
        {
            int bufferLength = IitOtdr.GetSorDataSize(_sorData);
            if (bufferLength == -1)
            {
                _rtuLogger.AppendLine("_sorData is null");
                return null;
            }
            byte[] buffer = new byte[bufferLength];

            var size = IitOtdr.GetSordata(_sorData, buffer, bufferLength);
            if (size == -1)
            {
                _rtuLogger.AppendLine("Error in GetLastSorData");
                return null;
            }
//            _rtuLogger.AppendLine("Measurement result received.");
            return buffer;
        }

        public byte[] ApplyAutoAnalysis(byte[] measBytes)
        {
            var measIntPtr = IitOtdr.SetSorData(measBytes);
            if (!IitOtdr.MakeAutoAnalysis(ref measIntPtr))
            {
                _rtuLogger.AppendLine("ApplyAutoAnalysis error.");
                return null;
            }
            var size = IitOtdr.GetSorDataSize(measIntPtr);
            byte[] resultBytes = new byte[size];
            IitOtdr.GetSordata(measIntPtr, resultBytes, size);

            IitOtdr.FreeSorDataMemory(measIntPtr);
            return resultBytes;
        }

        public OtdrDataKnownBlocks ApplyFilter(byte[] sorBytes, bool isFilterOn)
        {
            var sorData = SorData.FromBytes(sorBytes);
            sorData.IitParameters.Parameters = (IitBlockParameters)SetBitFlagInParameters((int)sorData.IitParameters.Parameters, IitBlockParameters.Filter, isFilterOn);
            return sorData;
        }

        public bool IsFilterOnInBase(byte[] sorBytes)
        {
            var sorData = SorData.FromBytes(sorBytes);
            return (sorData.IitParameters.Parameters & IitBlockParameters.Filter) != 0;
        }

        private int SetBitFlagInParameters(int parameters, IitBlockParameters parameter, bool flag)
        {
            return flag
                ? parameters | (int)parameter
                : parameters & (65535 ^ (int)parameter);
        }
    }
}
