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
            NpgsqlConnection.GlobalTypeMapper.UseJsonNet();

            NpgsqlConnection.GlobalTypeMapper.MapEnum<Sex>(Scheme + ".sex");
            NpgsqlConnection.GlobalTypeMapper.MapComposite<User>(Scheme + ".user_type");
            NpgsqlConnection.GlobalTypeMapper.MapComposite<Order>(Scheme + ".order_type");
            NpgsqlConnection.GlobalTypeMapper.MapComposite<OrderEntry>(Scheme + ".order_entry_type");
            NpgsqlConnection.GlobalTypeMapper.MapComposite<Operator>(Scheme + ".operator_type");
        }

        readonly string _connectionString;

        public DBClient(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<NpgsqlConnection> Connect()
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

        public async Task<(Operator Operator, string Password)> GetOperator(string login)
        {
            using (var connection = await Connect())
            using (var command = Call(connection, Scheme + ".get_operator"))
            {
                command.Parameters.AddWithValue("p_login", login);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (!await reader.ReadAsync())
                        return default;

                    var @operator = reader.GetFieldValue<Operator>(0);
                    var password = reader.GetFieldValue<string>(1);

                    return (@operator, password);
                }
            }
        }

        public async Task<Product> GetProduct(int id)
        {
            using (var connection = await Connect())
            using (var command = Call(connection, Scheme + ".get_product"))
            {
                command.Parameters.AddWithValue("p_id", id);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (!await reader.ReadAsync())
                        return default;

                    return new Product
                    {
                        ID = reader.GetFieldValue<int>(0),
                        Name = reader.GetFieldValue<string>(1),
                        CategoryID = reader.GetFieldValue<int>(2)
                    };
                }
            }
        }

        public async Task<List<Product>> GetProducts(IList<int> ids)
        {
            using (var connection = await Connect())
            using (var command = Call(connection, Scheme + ".get_products"))
            {
                command.Parameters.AddWithValue("p_id", ids);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    var result = new List<Product>();

                    while (await reader.ReadAsync())
                    {
                        result.Add(new Product
                        {
                            ID = reader.GetFieldValue<int>(0),
                            Name = reader.GetFieldValue<string>(1),
                            CategoryID = reader.GetFieldValue<int>(2)
                        });
                    }

                    return result;
                }
            }
        }

        public async Task<Category> GetCategory(int id)
        {
            using (var connection = await Connect())
            using (var command = Call(connection, Scheme + ".get_category"))
            {
                command.Parameters.AddWithValue("p_id", id);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (!await reader.ReadAsync())
                        return default;

                    return new Category
                    {
                        ID = reader.GetFieldValue<int>(0),
                        Name = reader.GetFieldValue<string>(1),
                        ParentID = reader.GetFieldValue<int?>(2)
                    };
                }
            }
        }

        public async Task<List<Category>> GetCategories(IList<int> ids)
        {
            using (var connection = await Connect())
            using (var command = Call(connection, Scheme + ".get_categories"))
            {
                command.Parameters.AddWithValue("p_id", ids);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    var result = new List<Category>();

                    while (await reader.ReadAsync())
                    {
                        result.Add(new Category
                        {
                            ID = reader.GetFieldValue<int>(0),
                            Name = reader.GetFieldValue<string>(1),
                            ParentID = reader.GetFieldValue<int?>(2)
                        });
                    }

                    return result;
                }
            }
        }

        public async Task<List<Category>> SearchCategory(string query, int count)
        {
            using (var connection = await Connect())
            using (var command = Call(connection, Scheme + ".search_category"))
            {
                command.Parameters.AddWithValue("p_prefix", query);
                command.Parameters.AddWithValue("p_limit", count);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    var result = new List<Category>();

                    while (await reader.ReadAsync())
                    {
                        result.Add(new Category
                        {
                            ID = reader.GetFieldValue<int>(0),
                            Name = reader.GetFieldValue<string>(1),
                            ParentID = reader.GetFieldValue<int?>(2)
                        });
                    }

                    return result;
                }
            }
        }

        public async Task<(Order Order, int[] ProductIDs)> GetOrder(int id)
        {
            using (var connection = await Connect())
            using (var command = Call(connection, Scheme + ".get_order"))
            {
                command.Parameters.AddWithValue("p_id", id);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (!await reader.ReadAsync())
                        return default;

                    var order = new Order
                    {
                        ID = reader.GetFieldValue<int>(0),
                        Day = reader.GetFieldValue<int>(1),
                        UserID = reader.GetFieldValue<int>(2)
                    };
                    var productIDs = reader.GetFieldValue<int[]>(3);

                    return (order, productIDs);
                }
            }
        }

        public async Task<User> GetUser(int id)
        {
            using (var connection = await Connect())
            using (var command = Call(connection, Scheme + ".get_user"))
            {
                command.Parameters.AddWithValue("p_id", id);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (!await reader.ReadAsync())
                        return default;

                    return new User
                    {
                        ID = reader.GetFieldValue<int>(0),
                        Age = reader.GetFieldValue<int>(1),
                        Sex = reader.GetFieldValue<Sex>(2)
                    };
                }
            }
        }

        public async Task<List<(Order Order, int[] ProductIDs)>> GetUserOrders(int userID, int offset, int limit)
        {
            using (var connection = await Connect())
            using (var command = Call(connection, Scheme + ".get_user_orders"))
            {
                command.Parameters.AddWithValue("p_user_id", userID);
                command.Parameters.AddWithValue("p_offset", offset);
                command.Parameters.AddWithValue("p_limit", limit);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    var result = new List<(Order Order, int[] ProductIDs)>();

                    while (await reader.ReadAsync())
                    {
                        var order = new Order
                        {
                            ID = reader.GetFieldValue<int>(0),
                            Day = reader.GetFieldValue<int>(1),
                            UserID = reader.GetFieldValue<int>(2)
                        };
                        var productIDs = reader.GetFieldValue<int[]>(3);

                        result.Add((order, productIDs));
                    }

                    return result;
                }
            }
        }
    }
}
