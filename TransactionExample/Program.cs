using System;
using System.Data.SqlClient;

namespace TransactionExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var connection = new SqlConnectionStringBuilder();
            connection.DataSource = @".\sql2017";
            connection.InitialCatalog = "Chapyar_Test";
            connection.UserID = "user";
            connection.Password = "";

            PrepareTestTable(connection.ConnectionString);

            var testBeginTransaction = new BeginTransactionSample(connection.ConnectionString);
            testBeginTransaction.StartTest();

            Console.ReadKey();
        }

        private static void PrepareTestTable(string connectionString)
        {
            var sqlcon = new SqlConnection(connectionString);
            var command = new SqlCommand();
            command.CommandText = "DROP TABLE test; create table test(id int, name varchar(1000));";
            command.Connection = sqlcon;
            try
            {
                sqlcon.Open();
                command.ExecuteScalar();
            }
            finally
            {
                sqlcon.Close();
            }
        }
    }

    class TransactionScopeSample
    {

    }

    class BeginTransactionSample
    {
        private readonly string connectionString;
        public BeginTransactionSample(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public void StartTest()
        {
            using (var sqlcn = new SqlConnection(connectionString))
            {
                sqlcn.Open();
                using (var transaction = sqlcn.BeginTransaction())
                {
                    try
                    {
                        var command = new SqlCommand();
                        command.CommandText = "INSERT INTO test (id, name) VALUES (5000, 'Test BeginTransaction statement')";
                        command.Connection = sqlcn;
                        command.Transaction = transaction;
                        command.ExecuteNonQuery();
                        transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("There is an error:\r\n {0}", e.Message);
                        transaction.Rollback();
                    }
                    finally
                    {
                        sqlcn.Close();
                    }
                }
            }
        }
    }
}
