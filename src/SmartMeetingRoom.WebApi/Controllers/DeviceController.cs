using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Microsoft.Azure.Devices.Common.Exceptions;
using AzureDevices = Microsoft.Azure.Devices;


namespace SmartMeetingRoom.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class DeviceController : Controller
    {
        private IotHubService _iotHubService;
        private DocumentDbServiceDevice _ddbService;

        public DeviceController()
        {
            _ddbService = new DocumentDbServiceDevice();
            _iotHubService = new IotHubService();
        }
        // GET: api/values
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            List<Device> devices = await _ddbService.GetAllDevices();
            HttpContext.Response.StatusCode = 201;
            return Json(devices);
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public IActionResult GetDeviceById(string id)
        {
            Device device = _ddbService.GetDevice(id);
            HttpContext.Response.StatusCode = 201;
            return Json(device);
        }

        // POST api/values
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] Device device)
        {
            AzureDevices.Device registeredDevice = await _iotHubService.RegisterDevice(device);
            await _ddbService.SaveAsync(device);
            HttpContext.Response.StatusCode = 201;
            return Json(new
            {
                deviceKey = registeredDevice.Authentication.SymmetricKey.PrimaryKey,
                deviceId = registeredDevice.Id
            });

        }
    }

    public class Device
    {

        public ObjectId id { get; set; }
        [BsonElement("DeviceId")]
        public string DeviceId { get; set; }
        [BsonElement("MeetingRoomId")]
        public string MeetingRoomId { get; set; }
        [BsonElement("CameraId")]
        public string CameraId { get; set; }
        [BsonElement("Confidence")]
        public double Confidence { get; set; }

    }
    public class DocumentDbServiceDevice
    {
        private IMongoDatabase _database { get; }
        internal IMongoCollection<Device> DeviceCollection { get; }
        public DocumentDbServiceDevice()
        {

            try
            {
                MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl("mongodb://smartmeet:password123@ds034807.mlab.com:34807/smartmeeting"));
                if (true)
                {
                    settings.SslSettings = new SslSettings { EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12 };
                }
                var mongoClient = new MongoClient(settings);
                _database = mongoClient.GetDatabase("smartmeeting");
                DeviceCollection = _database.GetCollection<Device>("devices");
            }
            catch (Exception ex)
            {
                throw new Exception("Can not access to db server.", ex);
            }
        }

        public Device GetDevice(string deviceId)
        {
            var id = new ObjectId(deviceId);
            IEnumerable<Device> deviceQuery = DeviceCollection.Find(d => d.id == id).ToEnumerable();
            Device selectedDevice = deviceQuery.FirstOrDefault();
            return selectedDevice;
        }

        public async Task<List<Device>> GetAllDevices()
        {
            List<Device> devices = await DeviceCollection.Find(new BsonDocument()).ToListAsync();
            return devices;
        }



        public async Task SaveAsync(Device device)
        {
            if (GetDevice(device.id.ToString()) == null)
            {
                await DeviceCollection.InsertOneAsync(device);
            }

        }




    }
    public class IotHubService
    {
        private readonly AzureDevices.RegistryManager _registryManager;

        public IotHubService()
        {
            _registryManager = AzureDevices.RegistryManager.CreateFromConnectionString("HostName=smrtmeetingIoTHub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=4wnwTD83Cx9wlJFlCghWHJ+LVqZ+PIM8d1h6XoG8pdA=");
        }

        public async Task<AzureDevices.Device> RegisterDevice(Device device)
        {
            AzureDevices.Device azureDevice;
            try
            {
                azureDevice = await _registryManager.AddDeviceAsync(new AzureDevices.Device(device.DeviceId));
            }
            catch (DeviceAlreadyExistsException)
            {
                azureDevice = await _registryManager.GetDeviceAsync(device.DeviceId);
            }

            return azureDevice;
        }
    }
}