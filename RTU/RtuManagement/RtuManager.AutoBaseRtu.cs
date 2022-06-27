using System;
using System.IO;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Newtonsoft.Json;

namespace Iit.Fibertest.RtuManagement
{
    public partial class RtuManager
    {
        private void DoAutoBaseMeasurementsForRtu(DoClientMeasurementDto dto)
        {
            IsRtuAutoBaseMode = true;
            _rtuIni.Write(IniSection.Monitoring, IniKey.IsRtuAutoBaseMode, true);

            MeasureAllPorts(dto);

            IsRtuAutoBaseMode = false;
            _rtuIni.Write(IniSection.Monitoring, IniKey.IsRtuAutoBaseMode, false);
            _serviceLog.AppendLine("All auto base measurements finished");
        }

        private void MeasureAllPorts(DoClientMeasurementDto dto)
        {
            var result = new ClientMeasurementResultDto().Initialize(dto);

            SaveDoClientMeasurementDto(dto);
            var portListCopy = dto.OtauPortDtoList.Select(i => i[0].Clone()).ToList();

            foreach (var currentPort in portListCopy)
            {
                if (!ToggleToPort(currentPort))
                    result.Set(currentPort, ReturnCode.RtuToggleToPortError);
                else if (!PrepareAutoLmaxMeasurement(dto))
                    result.Set(currentPort, ReturnCode.InvalidValueOfLmax);
                else
                    result = ClientMeasurementItself(dto, currentPort);

                var attempts = 3;
                var r2DWcfManager = new R2DWcfManager(_serverAddresses, _serviceIni, _serviceLog);
                while (!r2DWcfManager.SendClientMeasurementDone(result) && attempts-- > 0) { }

                _rtuLog.EmptyLine();

                dto.OtauPortDtoList.RemoveAt(0);
                SaveDoClientMeasurementDto(dto);
            }
        }

        # region Save-Load dto with auto base port list

        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All
        };
        private readonly string _autoBaseDtoFile = Utils.FileNameForSure(@"..\ini\", @"autobase.dto", false);

        private void SaveDoClientMeasurementDto(DoClientMeasurementDto dto)
        {
            try
            {
                var json = JsonConvert.SerializeObject(dto, JsonSerializerSettings);
                File.WriteAllText(_autoBaseDtoFile, json);
            }
            catch (Exception e)
            {
                _rtuLog.AppendLine($"AutoBase dto saving: {e.Message}");
            }
        }

        private DoClientMeasurementDto LoadDoClientMeasurementDto()
        {
            DoClientMeasurementDto result = null;
            try
            {
                var context = File.ReadAllText(_autoBaseDtoFile);
                result = JsonConvert.DeserializeObject<DoClientMeasurementDto>(context);
            }
            catch (Exception e)
            {
                _rtuLog.AppendLine($"AutoBase dto loading: {e.Message}");
            }

            return result;
        }

        #endregion

    }
}
