using System;
using System.Data.SqlClient;

namespace TransactionExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Runner.RunTest(new TransactionScopeSample());
            Console.ReadKey();
        }
    }

    static class Runner
    {
        public static void RunTest(ISample sample)
        {
            var connection = GetSqlConnection();
            PrepareTestTable(connection);
            sample.StartTest(connection, out object result);
            Console.WriteLine("========== test result: {0} rows", result);
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
        public void StartTest(string connectionString, out object result)
        {
            using (var scope = new System.Transactions.TransactionScope())
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand()) {
                command.CommandText = "INSERT INTO test (id, name) VALUES (5000, 'Test BeginTransaction statement')";
                command.Connection = connection;
                try
                {

                    Console.WriteLine("========= Transaction info:");
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\tLocal identifier: {0}, Status: {1}", System.Transactions.Transaction.Current.TransactionInformation.LocalIdentifier, System.Transactions.Transaction.Current.TransactionInformation.Status);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine();

                    connection.Open();
                    result = command.ExecuteNonQuery();
                    scope.Complete();
                }
                finally
                {
                    connection.Close();
                }
            }
        }
    }

    class BeginTransactionSample : ISample
    {
        public void StartTest(string connectionString, out object result)
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
                        result = command.ExecuteNonQuery();
                        transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        result = 0;
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
        void StartTest(string connectionString, out object result);
    }
}

