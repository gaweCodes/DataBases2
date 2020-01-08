using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace EmployeeReader
{
    internal static class Program
    {
        private static IDbConnection _connection;
        private static void Main()
        {
            try
            {
                const string connectionString = "Server=localhost;Database=sqlteacherdb;uid=demo;pwd=secret";
                OpenConnection(connectionString);
                Read();
                Console.Write("Exited regularly");
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                CloseConnection();
            }
        }
        private static void OpenConnection(string connectionString)
        {
            CloseConnection();
            _connection = new MySqlConnection(connectionString);
            _connection.Open();
        }
        private static void CloseConnection()
        {
            try
            {
                _connection?.Close();
                _connection = null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void Read()
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT mitarbeiternr, name, vorname from mitarbeiter;";
            using (var dataReader = cmd.ExecuteReader())
            {
                var dataRow = new object[dataReader.FieldCount];
                while (dataReader.Read())
                {
                    var colCount = dataReader.GetValues(dataRow);
                    for (var i = 0; i < colCount; i++)
                        Console.Write($"| {dataRow[i]} ");
                    Console.WriteLine("|");
                    Console.WriteLine();
                    Console.WriteLine($"| {dataReader["mitarbeiternr"]} " +
                                      $"| {dataReader["name"]} " +
                                      $"| {dataReader["vorname"]} |");
                }
            }
        }
    }
}
