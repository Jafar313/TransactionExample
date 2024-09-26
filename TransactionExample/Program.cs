using System;
using System.Data.SqlClient;

namespace TransactionExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Runner.RunTest(new BeginTransactionSample());
            Console.ReadKey();
        }
    }

    static class Runner
    {
        public static void RunTest(ISample sample)
        {
            var connection = GetSqlConnection();
            PrepareTestTable(connection);
            sample.StartTest(connection);
        }

        private static string GetSqlConnection()
        {
            var connection = new SqlConnectionStringBuilder();
            connection.DataSource = @".\sql2017";
            connection.InitialCatalog = "Chapyar_Test";
            connection.UserID = "user";
            connection.Password = "";
            return connection.ConnectionString;
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

    class TransactionScopeSample : ISample
    {
        public void StartTest(string connectionString)
        {
            throw new NotImplementedException();
        }
    }

    class BeginTransactionSample : ISample
    {
        public void StartTest(string connectionString)
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

    interface ISample
    {
        void StartTest(string connectionString);
    }
}

