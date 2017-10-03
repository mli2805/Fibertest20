using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.RtuManagement
{
    public class MoniResultOnDisk
    {
        public Guid Id { get; set; }
        public MonitoringResultDto Dto { get; set; }

        public IMyLog LogFile { get; set; }


        public MoniResultOnDisk(Guid id, MonitoringResultDto dto, IMyLog logFile)
        {
            Id = id;
            Dto = dto;
            LogFile = logFile;
        }

        private string GetFilename()
        {
            var appDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (appDir == null)
                return "";
            var storeDir = Path.Combine(appDir, @"..\ResultStore");

            if (!Directory.Exists(storeDir))
                Directory.CreateDirectory(storeDir);

            return Path.Combine(storeDir, $@"{Id}.dto");
        }
        public void Save()
        {
            try
            {
                using (Stream fStream = new FileStream(GetFilename(), FileMode.Create, FileAccess.Write))
                {
                    var binaryFormatter = new BinaryFormatter();
                    binaryFormatter.Serialize(fStream, Dto);
                }
            }
            catch (Exception e)
            {
                LogFile.AppendLine(e.Message);
            }
        }

        public bool Load()
        {
            try
            {
                using (Stream fStream = new FileStream(GetFilename(), FileMode.Open, FileAccess.Read))
                {
                    var binaryFormatter = new BinaryFormatter();
                    Dto = (MonitoringResultDto)binaryFormatter.Deserialize(fStream);
                    return true;
                }
            }
            catch (Exception e)
            {
                LogFile.AppendLine(e.Message);
                return false;
            }
        }

        public void Delete()
        {
            try
            {
                File.Delete(GetFilename());
            }
            catch (Exception e)
            {
                LogFile.AppendLine(e.Message);
            }
        }
    }
}