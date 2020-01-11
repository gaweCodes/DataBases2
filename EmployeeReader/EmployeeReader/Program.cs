using System;
using System.Data;
using System.Threading;
using MySql.Data.MySqlClient;

namespace EmployeeReader
{
    internal static class Program
    {
        private static IDbConnection _connection;
        // Darf kein IDbConnection und kein IDbCommand sein.
        private static MySqlCommand _asyncQueryCmd;
        private static bool _exited;
        private static MySqlCommand _cmdAsync;
        private static void Main()
        {
            try
            {
                // const string connectionString = "Server=localhost;Database=sqlteacherdb;uid=demo;pwd=secret";
                // OpenConnection(connectionString);
                var connectionStringBuilder = new MySqlConnectionStringBuilder
                {
                    Server = "localhost",
                    Database = "sqlteacherdb",
                    UserID = "demo",
                    Password = "secret",
                    Port = 3306
                };
                OpenConnection(connectionStringBuilder.ConnectionString);
                Count();
                Insert();
                Update();
                Read();
                Count();
                DeleteWithParameter();
                Count();
                StoredProcedure();
                StoredProcedureAsync();
                // ReadAsync();
                Console.Write("Exited regularly");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                CloseConnection();
            }
            Console.ReadLine();
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
            var queryCmd = _connection.CreateCommand();
            queryCmd.CommandText = "SELECT mitarbeiternr, name, vorname FROM mitarbeiter;";
            using (var dataReader = queryCmd.ExecuteReader())
            {
                var dataRow = new object[dataReader.FieldCount];
                while (dataReader.Read())
                {
                    var colCount = dataReader.GetValues(dataRow);
                    for (var i = 0; i < colCount; i++)  Console.Write($"| {dataRow[i]} ");
                    Console.WriteLine("|");
                    Console.WriteLine();
                    Console.WriteLine($"| {dataReader["mitarbeiternr"]} " +
                                      $"| {dataReader["name"]} " +
                                      $"| {dataReader["vorname"]} |");
                }
            }
        }
        private static void ReadAsync()
        {
            var conn = new MySqlConnection("Server=localhost;Database=sqlteacherdb;uid=demo;pwd=secret");
            conn.Open();
            _asyncQueryCmd = new MySqlCommand("SELECT mitarbeiternr, name, vorname FROM mitarbeiter;", conn);
            AsyncCmdEnded(_asyncQueryCmd.BeginExecuteReader());
        }
        private static void AsyncCmdEnded(IAsyncResult result)
        {
            using (var dataReader = _asyncQueryCmd.EndExecuteReader(result))
            {
                var dataRow = new object[dataReader.FieldCount];
                while (dataReader.Read())
                {
                    var colCount = dataReader.GetValues(dataRow);
                    for (var i = 0; i < colCount; i++) Console.Write($"| {dataRow[i]} ");
                    Console.WriteLine("|");
                }
            }
        }
        private static void Insert()
        {
            using (var insertCmd = _connection.CreateCommand())
            {
                insertCmd.CommandText = "INSERT INTO mitarbeiter(mitarbeiternr, vorname, name) VALUES (17, 'Gabriel', 'Weibel');";
                var affectedRows = insertCmd.ExecuteNonQuery();
                Console.WriteLine($"Affected Rows: {affectedRows}");
            }
        }
        private static void Update()
        {
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "UPDATE mitarbeiter SET gehalt = gehalt * 1.1";
                var affectedRows = cmd.ExecuteNonQuery();
                Console.WriteLine($"Affected Rows: {affectedRows}");
            }
        }
        private static void Count()
        {
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "SELECT count(*) FROM Mitarbeiter";
                var cnt = (long)cmd.ExecuteScalar();
                Console.WriteLine($"Count: {cnt}");
            }
        }
        private static void DeleteWithParameter()
        {
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM mitarbeiter WHERE mitarbeiternr = @ID";
                cmd.Parameters.Add(new MySqlParameter("@ID", MySqlDbType.Int32) { Value = 16 });
                var affectedRows = cmd.ExecuteNonQuery();
                Console.WriteLine($"Affected Rows: {affectedRows}");
            }
        }
        private static void StoredProcedure()
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "SayHello";
            var p1 = new MySqlParameter("name", MySqlDbType.VarChar)
            {
                Value = "Thomas", Direction = ParameterDirection.Input
            };
            var p2 = new MySqlParameter("hello", MySqlDbType.VarChar) {Direction = ParameterDirection.Output};
            cmd.Parameters.Add(p1);
            cmd.Parameters.Add(p2);
            cmd.ExecuteNonQuery();
            Console.WriteLine($"Result: {p2.Value}");
        }
        private static void StoredProcedureAsync()
        {
            using (var conn = new MySqlConnection("Server=localhost;Database=sqlteacherdb;uid=demo;pwd=secret"))
            {
                try
                {
                    conn.Open();
                    _cmdAsync = new MySqlCommand
                    {
                        Connection = conn, CommandType = CommandType.StoredProcedure, CommandText = "SayHello"
                    };
                    var p1 = new MySqlParameter("name", MySqlDbType.VarChar)
                    {
                        Value = "Thomas", Direction = ParameterDirection.Input
                    };
                    var p2 = new MySqlParameter("hello", MySqlDbType.VarChar) {Direction = ParameterDirection.Output};

                    _cmdAsync.Parameters.Add(p1);
                    _cmdAsync.Parameters.Add(p2);
                    _cmdAsync.BeginExecuteNonQuery(Callback, null);
                    while (!_exited)
                    {
                        Console.Write(".");
                        Thread.Sleep(500);
                    }
                    Console.WriteLine($"Result: {p2.Value}");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
        private static void Callback(IAsyncResult ar)
        {
            _cmdAsync.EndExecuteNonQuery(ar);
            _exited = true;
        }
    }
}
