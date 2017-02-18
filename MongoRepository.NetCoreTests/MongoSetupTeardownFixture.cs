using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace MongoRepository.NetCoreTests
{
    /*
     * Extend this class to setup a test class that shares context throughout all tests, which is to say,
     * MongoFixture() will be run when the test class is instantiated, and Dispose() will be run when all
     * tests are completed.
     * e.g. public class MyTests : IClassFixture<MongoSetupTeardownFixture>
     */
    public class MongoSetupTeardownFixture : IDisposable
    {
        public MongoSetupTeardownFixture()
        {
            this.DropDB();
        }

        public void Dispose()
        {
            this.DropDB();
        }

        private void DropDB()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            var configuration = builder.Build();
            var url = new MongoUrl(configuration["MongoServerSettings:connectionString"]);
            // var url = new MongoUrl(ConfigurationManager.ConnectionStrings["MongoServerSettings"].ConnectionString);
            var client = new MongoClient(url);
            client.DropDatabase(url.DatabaseName);
        }
    }
}
