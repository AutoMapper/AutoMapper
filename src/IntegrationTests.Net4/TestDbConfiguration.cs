using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace AutoMapper.IntegrationTests.Net4
{
    public class TestDbConfiguration : DbConfiguration
    {
        public TestDbConfiguration()
        {
            var env = Environment.GetEnvironmentVariable("APPVEYOR");
            if (env != null)
            {
                SetDefaultConnectionFactory(
                    new SqlConnectionFactory(@"Server=(local)\SQL2014;Database=master;User ID=sa;Password=Password12!"));
            }
            else
            {
                SetDefaultConnectionFactory(new LocalDbConnectionFactory("v12.0"));
            }
        }
    }
}