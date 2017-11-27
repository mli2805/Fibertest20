using System.Data.Entity;

namespace Iit.Fibertest.Client
{
    class SQLiteDbConfiguration : DbConfiguration
    {
        public SQLiteDbConfiguration()
        {
            AddDependencyResolver(new SQLiteDbDependencyResolver());
        }
    }
}