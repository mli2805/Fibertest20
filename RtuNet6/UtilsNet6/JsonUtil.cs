using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Iit.Fibertest.UtilsNet6;

public static class JsonUtil
{
    public static string ToCamelCaseJson(this object o)
    {
        return JsonConvert.SerializeObject(o,
            new JsonSerializerSettings() {
                ContractResolver = new DefaultContractResolver() { NamingStrategy = new CamelCaseNamingStrategy() }
            });
    }
}