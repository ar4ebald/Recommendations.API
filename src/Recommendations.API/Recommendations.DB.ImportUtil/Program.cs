using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using Recommendations.DB.ImportUtil.Model;
using Recommendations.Model;

namespace Recommendations.DB.ImportUtil
{
    class Program
    {
        const int BatchSize = 1024 * 32;

        static async Task Main(string[] args)
        {
            var connectionString = args[0];
            var root = args[1];

            var client = new DBClient(connectionString);

            await ImportUsers(Path.Combine(root, "users.csv"), client);
            await ImportProducts(Path.Combine(root, "products.csv"), client);
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
            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader))
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

            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader))
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

            int total = 0;
            foreach (var batch in ToBatches(products))
            {
                await client.AddProduct(
                    batch.ConvertAll(x => x.ProductID),
                    batch.ConvertAll(x => x.ProductName),
                    batch.ConvertAll(x => x.AisleID)
                );

                total += batch.Count;
                Console.Write($"{path} products: {total}\r");
            }

            Console.WriteLine();
        }

        static async Task ImportOrders(string path, DBClient client)
        {
            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader))
            {
                int total = 0;
                var addedOrdersIDs = new HashSet<int>();
                var orders = new List<Order>();

                foreach (var batch in ToBatches(csv.GetRecords<CSVOrder>()))
                {
                    foreach (var x in batch)
                    {
                        if (!addedOrdersIDs.Add(x.OrderID))
                            continue;

                        orders.Add(new Order
                        {
                            ID = x.OrderID,
                            UserID = x.UserID,
                            Day = x.Day
                        });
                    }

                    await client.AddOrder(orders);
                    orders.Clear();

                    await client.AddOrderEntry(batch.ConvertAll(x => new OrderEntry
                    {
                        OrderID = x.OrderID,
                        ProductID = x.ProductID
                    }));

                    total += batch.Count;
                    Console.Write($"{path} orders: {addedOrdersIDs.Count}, entries: {total}\r");
                }

                Console.WriteLine();
            }
        }
    }
}
