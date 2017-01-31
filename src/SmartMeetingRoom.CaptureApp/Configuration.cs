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
        public static string IotHubUrl => "smartmeetingiothub.azure-devices.net";
        public static string StorageConnectionString
            =>
            "DefaultEndpointsProtocol=https;AccountName=smrtmeetdevi7v3j2pn2fltq;AccountKey=Sr4wPu8eMQWrEiqhAs5v0PTw64CmSldbEwatSmbnXIujKWbep2CWefoVGlDZhCYOQguGzrNWr1gY210eaPI9gA=="
            ;
        public static string StorageContainer => "faces";

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