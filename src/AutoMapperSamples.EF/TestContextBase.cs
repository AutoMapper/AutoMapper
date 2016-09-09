using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoMapperSamples.EF
{

    public interface ITestContext
    {
        void Seed();

        bool IsOracle { get; }
        string SchemaName { get; }
    }

    /// <summary>
    /// Base DbContext used by all tests.  Handles special requirements for initializing an Oracle database.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class TestContextBase<T> : DbContext
        where T : DbContext, ITestContext
    {
        /// <summary>
        /// Default constructor that sets the Connection String Name to TestContext and uses the default initializer.
        /// </summary>
        public TestContextBase()
            : base("TestContext")
        {
            Database.SetInitializer(new ContentInitializer<T>());
            Database.Log = log => System.Diagnostics.Debug.WriteLine(log);
            Database.Initialize(false);
        }

        /// <summary>
        /// Constructor that only sets the Connection String Name - database initializer is not set.
        /// </summary>
        /// <param name="nameOrConnectionString"></param>
        public TestContextBase(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        /// <summary>
        /// Consatructor that uses the given dbConnection and configures the default initializer
        /// </summary>
        /// <param name="dbConnextion"></param>
        public TestContextBase(DbConnection dbConnextion)
            : base(dbConnextion, false)
        {
            Database.SetInitializer(new ContentInitializer<T>());
            Database.Log = log => System.Diagnostics.Debug.WriteLine(log);
            Database.Initialize(false);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            if (IsOracle)
            {
                //  For Oracle, we must set the default schema or EF will try to use "dbo" which will not be valid.
                //  And it must be upper case.
                modelBuilder.HasDefaultSchema(SchemaName);
            }

            base.OnModelCreating(modelBuilder);
        }

        public abstract void Seed();

        public bool IsOracle
        {
            get { return Database.Connection.GetType().FullName.Contains("Oracle"); }
        }

        public string SchemaName
        {
            get
            {
                    return "dbo";

            }
        }
    }

    public class ContentInitializer<T> : DropCreateDatabaseAlways<T>
        where T : DbContext, ITestContext
    {
        #region Initialize/Seed


        protected override void Seed(T context)
        {
            context.Seed();
        }

        #endregion
        

    }
}
