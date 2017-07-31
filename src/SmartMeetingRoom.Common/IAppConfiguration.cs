
namespace SmartMeetingRoom.Common
{
    public interface IAppConfiguration
    {

        string Endpoint { get; }
        string Key { get; }
        string DatabaseName { get; }
        string DevicesCollectionName { get; }
        string EmployeesCollectionName { get; }
        string CalendersCollectionName { get; }
        string IotHubConnectionString { get; }
        string StorageConnectionString { get; }
        string MongoDBConnectionString { get; }
        string MongoDBDatabaseName { get; }
        bool IsMongoSSL { get; }
    }
}
