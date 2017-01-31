using System.Threading.Tasks;
using Microsoft.Azure.Devices.Common.Exceptions;
using SmartMeetingRoom.Common.DTO;
using AzureDevices = Microsoft.Azure.Devices;
using SmartMeetingRoom.Common;

namespace SmartMeetingRoom.Common.Services
{
    public class IotHubService
    {
        private readonly AzureDevices.RegistryManager _registryManager;

        public IotHubService(IAppConfiguration config)
        {
            _registryManager = AzureDevices.RegistryManager.CreateFromConnectionString(config.IotHubConnectionString);
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