﻿using System.Collections.Generic;
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
            NpgsqlConnection.GlobalTypeMapper.MapComposite<User>(Scheme + ".user_type");
            NpgsqlConnection.GlobalTypeMapper.MapComposite<Order>(Scheme + ".order_type");
            NpgsqlConnection.GlobalTypeMapper.MapComposite<OrderEntry>(Scheme + ".order_entry_type");
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

        public async Task AddUser(IList<User> users)
        {
            using (var connection = await Connect())
            using (var command = Call(connection, Scheme + ".add_user"))
            {
                command.Parameters.AddWithValue("p_users", users);
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task AddCategory(IList<int> ids, IList<string> names, IList<int> parentIDs)
        {
            using (var connection = await Connect())
            using (var command = Call(connection, Scheme + ".add_category"))
            {
                command.Parameters.AddWithValue("p_id", ids);
                command.Parameters.AddWithValue("p_name", names);
                command.Parameters.AddWithValue("p_parent_id", parentIDs);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task AddProduct(IList<int> ids, IList<string> names, IList<int> categoryIDs)
        {
            using (var connection = await Connect())
            using (var command = Call(connection, Scheme + ".add_product"))
            {
                command.Parameters.AddWithValue("p_id", ids);
                command.Parameters.AddWithValue("p_name", names);
                command.Parameters.AddWithValue("p_category_id", categoryIDs);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task AddOrder(IList<Order> orders)
        {
            using (var connection = await Connect())
            using (var command = Call(connection, Scheme + ".add_order"))
            {
                command.Parameters.AddWithValue("p_orders", orders);
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task AddOrderEntry(IList<OrderEntry> entries)
        {
            using (var connection = await Connect())
            using (var command = Call(connection, Scheme + ".add_order_entry"))
            {
                command.Parameters.AddWithValue("p_entries", entries);
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}
