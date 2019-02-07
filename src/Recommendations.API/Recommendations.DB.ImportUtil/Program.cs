namespace Recommendations.DB.ImportUtil
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionString = args[0];
            var root = args[1];

            var client = new DBClient(connectionString);
        }
    }
}
