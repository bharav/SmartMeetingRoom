namespace SmartMeetingRoom.CaptureApp
{
    public interface IConfiguration
    {
        string BackendUrl { get; }
        string IotHubUrl { get; }

        string StorageConnectionString { get; }

        string StorageContainer { get; }
    }

    internal class StaticConfiguration : IConfiguration
    {
        //public static string BackendUrl => "http://smrtmeetdev-backendi7v3j2pn2fltq.azurewebsites.net/";
        public static string BackendUrl => "http://localhost:53673";
        public static string IotHubUrl => "smrtmeetingIoTHub.azure-devices.net";
        public static string StorageConnectionString
            =>
            "DefaultEndpointsProtocol=https;AccountName=smrtmeetdevhofio3ys3jhne;AccountKey=afhxNu4Sz2eDJDVwNziyyjFOUOEiA4Apaf6LuDNGKBF6f1ln2PyFMgc4cDp8R85jjgwLdOkSIZzjNotcEbunPA=="
            ;
        public static string StorageContainer => "faces/9abfd1aa-a701-9f3f-f72d-84496ed34b74";

        string IConfiguration.IotHubUrl
        {
            get { return IotHubUrl; }
        }

        string IConfiguration.StorageConnectionString
        {
            get { return StorageConnectionString; }
        }

        string IConfiguration.StorageContainer
        {
            get { return StorageContainer; }
        }

        string IConfiguration.BackendUrl
        {
            get { return BackendUrl; }
        }
    }
}