using Microsoft.Extensions.Configuration;
using SmartMeetingRoom.Common;

namespace SmartMeetingRoom.Backend
{
    public class AppConfiguration : IAppConfiguration
    {
        public AppConfiguration(IConfigurationRoot configuration)
        {
            Endpoint = configuration["DocumentDb:endpoint"];
            Key = configuration["DocumentDb:key"];
            DatabaseName = configuration["DocumentDb:database"];
            DevicesCollectionName = configuration["DocumentDb:deviceCollection"];
            EmployeesCollectionName = configuration["DocumentDb:employeeCollection"];
            CalendersCollectionName = configuration["DocumentDb:calenderCollection"];
            IotHubConnectionString = configuration["IotHub:connectionString"];
            StorageConnectionString = configuration["Storage:connectionString"];
            MongoDBConnectionString = configuration["MongoDB:connectionString"];
            IsMongoSSL = bool.Parse(configuration["MongoDB:isMongoSSL"]);
            MongoDBDatabaseName = configuration["MongoDB:databaseName"];
        }

        public string Endpoint { get; }
        public string Key { get; }
        public string DatabaseName { get; }
        public string DevicesCollectionName { get; }
        public string EmployeesCollectionName { get; }
        public string CalendersCollectionName { get; }
        public string IotHubConnectionString { get; }
        public string StorageConnectionString { get; }
        public string MongoDBConnectionString { get; }
        public bool IsMongoSSL { get; }
        public string MongoDBDatabaseName { get; }
        

            
            
    }
}