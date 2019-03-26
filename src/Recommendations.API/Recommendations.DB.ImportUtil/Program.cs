using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Recommendations.DB.ImportUtil.Model;
using Recommendations.Model;

namespace Recommendations.DB.ImportUtil
{
    class Program
    {
        const int BatchSize = 1024 * 32;

        static readonly Configuration _config;

        static Program()
        {
            _config = new Configuration(CultureInfo.GetCultureInfo("ru-RU"));
        }

        static async Task Main(string[] args)
        {
            var connectionString = args[0];
            var root = args[1];

            var client = new DBClient(connectionString);

            await ImportUsers(Path.Combine(root, "users.csv"), client);
            await ImportProducts(Path.Combine(root, "products_with_stats_20.csv"), client);
            await ImportOrders(Path.Combine(root, "orders.csv"), client);
        }

        static IEnumerable<List<T>> ToBatches<T>(IEnumerable<T> source)
        {
            var buffer = new List<T>();
            foreach (var item in source)
            {
                buffer.Add(item);
                if (buffer.Count < BatchSize)
                    continue;

                yield return buffer;
                buffer.Clear();
            }

            if (buffer.Count != 0)
                yield return buffer;
        }

        static async Task ImportUsers(string path, DBClient client)
        {
            using (var reader = new StreamReader(path, Encoding.UTF8))
            using (var csv = new CsvReader(reader, _config))
            {
                int total = 0;

                foreach (var batch in ToBatches(csv.GetRecords<CSVUser>()))
                {
                    await client.AddUser(batch.ConvertAll(x => new User
                    {
                        ID = x.ID,
                        Age = x.Age,
                        Sex = x.Sex == "M" ? Sex.Male : Sex.Female
                    }));

                    total += batch.Count;
                    Console.Write($"{path} users: {total}\r");
                }

                Console.WriteLine();
            }
        }

        static async Task ImportProducts(string path, DBClient client)
        {
            CSVProduct[] products;

            using (var reader = new StreamReader(path, Encoding.UTF8))
            using (var csv = new CsvReader(reader, new Configuration(_config.CultureInfo) { Delimiter = "," }))
                products = csv.GetRecords<CSVProduct>().ToArray();

            var departmentIDsOffset = products.Max(x => x.AisleID) + 1;


            var departments = products
                .Select(x => (ID: x.DepartmentID + departmentIDsOffset, Name: x.Department))
                .Distinct()
                .ToList();

            var aisles = products
                .Select(x => (ID: x.AisleID, Name: x.Aisle, ParentID: x.DepartmentID + departmentIDsOffset))
                .Distinct()
                .ToList();


            var departmentIDs = departments.ConvertAll(x => x.ID);
            var departmentNames = departments.ConvertAll(x => x.Name);
            var departmentParentIDs = departments.ConvertAll(x => 0);

            var aisleIDs = aisles.ConvertAll(x => x.ID);
            var aisleNames = aisles.ConvertAll(x => x.Name);
            var aisleParentIDs = aisles.ConvertAll(x => x.ParentID);


            await client.AddCategory(departmentIDs, departmentNames, departmentParentIDs);
            await client.AddCategory(aisleIDs, aisleNames, aisleParentIDs);

            Console.WriteLine($"{path} departments: {departments.Count}");
            Console.WriteLine($"{path} aisles: {aisles.Count}");

            var separators = new[] { ',', ' ', '[', ']' };

            using (var connection = await client.Connect())
            using (var writer = connection.BeginBinaryImport("COPY rdb.product (id, name, category_id, age, sex, purchased_with) FROM STDIN (FORMAT BINARY)"))
            {
                foreach (var product in products)
                {
                    var purchasedWith = product.PurchasedWith
                        .Substring(1, product.PurchasedWith.Length - 2)
                        .Split(separators, StringSplitOptions.RemoveEmptyEntries)
                        .Select(int.Parse)
                        .ToArray();

                    writer.StartRow();
                    writer.Write(product.ProductID);
                    writer.Write(product.ProductName);
                    writer.Write(product.AisleID);
                    writer.Write(product.Age);
                    writer.Write(product.Sex == "M" ? Sex.Male : Sex.Female);
                    writer.Write(purchasedWith);
                }

                writer.Complete();
            }
        }

        static async Task ImportOrders(string path, DBClient client)
        {
            using (var reader = new StreamReader(path, Encoding.UTF8))
            using (var csv = new CsvReader(reader, _config))
            using (var orderConnection = await client.Connect())
            using (var e = csv.GetRecords<CSVOrder>().GetEnumerator())
            {
                var addedOrdersIDs = new HashSet<int>();
                int total = 0;

                const string copyOrderCommand = "COPY rdb.order (id, user_id, day) FROM STDIN (FORMAT BINARY)";

                bool haveMore;
                do
                {
                    using (var orderWriter = orderConnection.BeginBinaryImport(copyOrderCommand))
                    {
                        while ((haveMore = e.MoveNext()) && ++total % 100000 != 0)
                        {
                            if (addedOrdersIDs.Add(e.Current.OrderID))
                            {
                                orderWriter.StartRow();
                                orderWriter.Write(e.Current.OrderID);
                                orderWriter.Write(e.Current.UserID);
                                orderWriter.Write(e.Current.Day);
                            }
                        }

                        orderWriter.Complete();
                        Console.Write($"{path} users: {total}\r");
                    }
                } while (haveMore);

                Console.WriteLine();
            }

            using (var reader = new StreamReader(path, Encoding.UTF8))
            using (var csv = new CsvReader(reader))
            using (var entryConnection = await client.Connect())
            using (var e = csv.GetRecords<CSVOrder>().GetEnumerator())
            {
                int total = 0;

                const string entryOrderCommand = "COPY rdb.order_entry (order_id, product_id) FROM STDIN (FORMAT BINARY)";

                bool haveMore;
                do
                {
                    using (var entryWriter = entryConnection.BeginBinaryImport(entryOrderCommand))
                    {
                        while ((haveMore = e.MoveNext()) && ++total % 100000 != 0)
                        {
                            entryWriter.StartRow();
                            entryWriter.Write(e.Current.OrderID);
                            entryWriter.Write(e.Current.ProductID);
                        }

                        entryWriter.Complete();
                        Console.Write($"{path} users: {total}\r");
                    }

                } while (haveMore);

                Console.WriteLine();
            }
        }
    }
}
