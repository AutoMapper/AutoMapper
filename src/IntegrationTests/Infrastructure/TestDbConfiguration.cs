using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace AutoMapper.IntegrationTests.Net4
{
    public class TestDbConfiguration : DbConfiguration
    {
        public TestDbConfiguration()
        {
            SetDefaultConnectionFactory(new LocalDbConnectionFactory("MSSQLLocalDB"));
        }
    }
}