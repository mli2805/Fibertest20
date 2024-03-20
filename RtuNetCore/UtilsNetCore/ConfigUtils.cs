using Iit.Fibertest.UtilsLib;
using Newtonsoft.Json;

namespace Iit.Fibertest.UtilsNetCore;

public static class ConfigUtils
{
    public static void EnsureCreation<T>(string filename) where T : new()
    {
        var empty = new T();

        if (!File.Exists(filename))
        {
            var directoryName = Path.GetDirectoryName(filename);
            if (!Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName!);
            File.WriteAllText(filename, JsonConvert.SerializeObject(empty));
        }

        // если не хватает какого-либо пол€ -
        //  при десериализации создаст со значением по умолчанию определенным в классе конфига и запишет в файл
        //  пол€ конфиг классов не должны быть nullable
        // если есть какие-то устаревшие пол€ -
        //  они будут проигнорированы при десериализации и не попадут в экземпл€р класса в пам€ти
        var json = File.ReadAllText(filename);
        var config = JsonConvert.DeserializeObject<T>(json);
        if (config == null) return;
        File.WriteAllText(filename, JsonConvert.SerializeObject(config));
    }

    public static T GetConfigManually<T>(string fileName) where T : new()
    {
        var configFolder = Path.Combine(FileOperations.GetMainFolder(), "config");
        var configFile = Path.Combine(configFolder, fileName);
        if (!Directory.Exists(configFolder) || !File.Exists(configFile))
            return new T();

        var content = File.ReadAllText(configFile);
        object? o = JsonConvert.DeserializeObject(content);
        return o != null ? (T)o : new T();
    }
}