using System;
using Npgsql;

namespace Recommendations.DB
{
    public sealed class DBClient
    {
        readonly string _connectionString;

        public DBClient(string connectionString)
        {
            _connectionString = connectionString;
        }


    }
}
