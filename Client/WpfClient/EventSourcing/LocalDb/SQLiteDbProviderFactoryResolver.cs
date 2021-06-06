using System.Data.Common;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Infrastructure;
using System.Data.SQLite;
using System.Data.SQLite.EF6;

namespace Iit.Fibertest.Client
{
    class SqLiteDbProviderFactoryResolver : IDbProviderFactoryResolver
    {
        public static readonly SqLiteDbProviderFactoryResolver Instance = new SqLiteDbProviderFactoryResolver();
        private SqLiteDbProviderFactoryResolver() { }
        public DbProviderFactory ResolveProviderFactory(DbConnection connection)
        {
            if (connection is SQLiteConnection) return SQLiteProviderFactory.Instance;
            if (connection is EntityConnection) return EntityProviderFactory.Instance;
            return null;
        }
    }
}