using System;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Data.SqlClient;

namespace TestEntityFramework472
{
    public class MyDbConfiguration : DbConfiguration
    {
        public MyDbConfiguration()
        {
            SetExecutionStrategy("System.Data.SqlClient", () => new MySqlAzureExecutionStrategy());
        }
    }

    public class MySqlAzureExecutionStrategy : SqlAzureExecutionStrategy
    {
        public MySqlAzureExecutionStrategy()
            : base()
        {
        }

        protected override bool ShouldRetryOn(Exception exception)
        {
            var sqlException = exception as SqlException;
            if (sqlException == null)
            {
                var innerException = exception;
                while (innerException.InnerException != null)
                {
                    innerException = innerException.InnerException;
                }
                Console.WriteLine($"{DateTime.UtcNow.ToString("HH:mm:ss")} ShouldRetryOn {innerException.GetType()}: {innerException.Message}");
            }
            else
            {
                var baseException = sqlException.GetBaseException();
                Console.WriteLine($"{DateTime.UtcNow.ToString("HH:mm:ss")} ShouldRetryOn {baseException.GetType()}: {baseException.Message}");
            }

            return base.ShouldRetryOn(exception);
        }
    }
}
