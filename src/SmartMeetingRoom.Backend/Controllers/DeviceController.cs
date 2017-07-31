using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SmartMeetingRoom.Common.Services;
using SmartMeetingRoom.Common.DTO;
using ADevice = Microsoft.Azure.Devices;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace SmartMeetingRoom.Backend.Controllers
{
    [Route("api/[controller]")]
    public class DeviceController : Controller
    {
        private IotHubService _iotHubService;
        private MongoDBService _ddbService;

        public DeviceController(MongoDBService ddbService,IotHubService IotHubService)
        {
            _ddbService = ddbService;
            _iotHubService = IotHubService;
        }
        // GET: api/values
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            List<Device> devices = await _ddbService.GetAllDevices();
            HttpContext.Response.StatusCode = 201;
            return Json(devices);
        }

        
        [HttpGet("{id}")]
        public IActionResult GetDeviceById(string id)
        {
            Device device = _ddbService.GetDevice(id);
            HttpContext.Response.StatusCode = 201;
            return Json(device);
        }

        /// <summary>
        /// Register device with device details 
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        // POST api/values
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] Device device)
        {
            ADevice.Device registeredDevice = await _iotHubService.RegisterDevice(device);
            await _ddbService.SaveAsync(device);
            HttpContext.Response.StatusCode = 201;
            return Json(new
            {
                deviceKey = registeredDevice.Authentication.SymmetricKey.PrimaryKey,
                deviceId = registeredDevice.Id
            });

        }
    }
}
