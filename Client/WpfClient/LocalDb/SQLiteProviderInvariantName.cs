using System.Data.Entity.Infrastructure;

namespace Iit.Fibertest.Client
{
    public class SqLiteProviderInvariantName : IProviderInvariantName
    {
        public static readonly SqLiteProviderInvariantName Instance = new SqLiteProviderInvariantName();
        private SqLiteProviderInvariantName() { }
        public const string ProviderName = "System.Data.SQLite.EF6";
        public string Name { get { return ProviderName; } }
    }
}