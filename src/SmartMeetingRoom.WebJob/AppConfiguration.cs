using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartMeetingRoom.Common;
using System.Configuration;

namespace SmartMeetingRoom.WebJob
{
    public class AppConfiguration: IAppConfiguration
    {
        public AppConfiguration()
        {
            Endpoint = ConfigurationManager.AppSettings["DocumentDBEndpoint"];
            Key = ConfigurationManager.AppSettings["DocumentDBKey"];
            DatabaseName = ConfigurationManager.AppSettings["DocumentDBDatabaseName"];
            IotHubConnectionString = ConfigurationManager.AppSettings["IotHubConnectionString"];
            StorageConnectionString = ConfigurationManager.AppSettings["StorageConnectionString"];
            MongoDBConnectionString = ConfigurationManager.AppSettings["MongoDBConnectionString"];
            MongoDBDatabaseName = ConfigurationManager.AppSettings["MongoDBDatabaseName"];
            IsMongoSSL = bool.Parse(ConfigurationManager.AppSettings["IsMongoSSL"]);
            DevicesCollectionName = "devices";
            EmployeesCollectionName = "employees";
            CalendersCollectionName = "calenders";

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
        public string MongoDBDatabaseName { get; }
        public bool IsMongoSSL { get; }
    }
}
