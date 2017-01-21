using Microsoft.Extensions.Configuration;
using SmartMeetingRoom.Common;

namespace SmartMeetingRoom.Backend
{
    internal class AppConfiguration : IAppConfiguration
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
        }

        public string Endpoint { get; }
        public string Key { get; }
        public string DatabaseName { get; }
        public string DevicesCollectionName { get; }
        public string EmployeesCollectionName { get; }
        public string CalendersCollectionName { get; }
        public string IotHubConnectionString { get; }
    }
}