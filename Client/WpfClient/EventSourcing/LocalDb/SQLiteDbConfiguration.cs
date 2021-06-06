using System.Data.Entity;

namespace Iit.Fibertest.Client
{
    class SqLiteDbConfiguration : DbConfiguration
    {
        public SqLiteDbConfiguration()
        {
            AddDependencyResolver(new SqLiteDbDependencyResolver());
        }
    }
}