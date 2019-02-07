using System;
using System.Data;
using System.Threading.Tasks;
using Npgsql;
using Recommendations.Model;

namespace Recommendations.DB
{
    public sealed class DBClient
    {
        const string Scheme = "rdb";

        static DBClient()
        {
            NpgsqlConnection.GlobalTypeMapper.MapEnum<Sex>(Scheme + ".sex");
        }

        readonly string _connectionString;

        public DBClient(string connectionString)
        {
            _connectionString = connectionString;
        }

        async Task<NpgsqlConnection> Connect()
        {
            var connection = new NpgsqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                return connection;
            }
            catch
            {
                connection.Dispose();
                throw;
            }
        }

        NpgsqlCommand Call(NpgsqlConnection connection, string name)
        {
            var command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = name;
            return command;
        }

        public async Task AddUser(int userID, int age, Sex sex)
        {
            using (var connection = await Connect())
            using (var command = Call(connection, Scheme + ".add_user"))
            {
                command.Parameters.AddWithValue("p_id", userID);
                command.Parameters.AddWithValue("p_age", age);
                command.Parameters.AddWithValue("p_sex", sex);

                await command.ExecuteNonQueryAsync();
            }
        }
    }
}
