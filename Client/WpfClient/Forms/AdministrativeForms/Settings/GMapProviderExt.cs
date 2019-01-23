using GMap.NET.MapProviders;

namespace Iit.Fibertest.Client
{
    public static class GMapProviderExt
    {
        public static GMapProvider Get(string providerName)
        {
            switch (providerName)
            {
                case "OpenStreetMap":
                {
                    return GMapProviders.OpenStreetMap;
                }
                case "GoogleMap":
                {
                    return GMapProviders.GoogleMap;
                }
                case "YandexMap":
                {
                    return GMapProviders.YandexMap;
                }
                default:
                    return GMapProviders.EmptyProvider;
            }
        }
    }
}