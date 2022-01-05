using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

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