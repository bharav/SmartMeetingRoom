using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Windows.Storage;

namespace SmartMeetingRoom.CaptureApp
{
    internal class AzureImagePersister : IImagePersiter
    {
        private readonly DeviceClient _deviceClient;
        private readonly IConfiguration _configuration;
        private readonly IDeviceConfiguration _deviceConfiguration;

        public AzureImagePersister(IConfiguration configuration, IDeviceConfiguration deviceConfiguration)
        {
            try
            {
                _configuration = configuration;
                _deviceConfiguration = deviceConfiguration;
                _deviceClient = DeviceClient.Create(configuration.IotHubUrl,
                    new DeviceAuthenticationWithRegistrySymmetricKey(_deviceConfiguration.DeviceIdString,
                        _deviceConfiguration.DeviceKey));
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
            }
        }
        /// <summary>
        /// Upload meeting continuation images
        /// </summary>
        /// <param name="faceImages"></param>
        /// <param name="cameraId"></param>
        /// <returns></returns>
        public async Task PersistAsync(IEnumerable<Stream> faceImages, string cameraId)
        {
            foreach (Stream faceImage in faceImages)
            {
                Guid blobGuid = Guid.NewGuid();

                var data = new
                {
                    deviceId = _deviceConfiguration.DeviceId,
                    blobName = blobGuid + ".jpg",
                    cameraId = cameraId,
                    registration = "No"
                };
                string messageString = JsonConvert.SerializeObject(data);
                Message message = new Message(Encoding.ASCII.GetBytes(messageString));

                await UploadImageToBlobAsync($"{data.blobName}", faceImage);
                Debug.WriteLine("Image uploaded.");

                await _deviceClient.SendEventAsync(message);
                Debug.WriteLine("Message sent.");

                faceImage.Dispose();
            }
        }
        /// <summary>
        /// Upload Image for registration
        /// </summary>
        /// <param name="faceImage"></param>
        /// <param name="cameraId"></param>
        /// <param name="empName"></param>
        /// <param name="empDeptName"></param>
        /// <returns></returns>
        public async Task PersistAsync(Stream faceImage, string cameraId, string empName, string empDeptName)
        {
            Guid blobGuid = Guid.NewGuid();

            var data = new
            {
                deviceId = _deviceConfiguration.DeviceId,
                blobName = blobGuid + ".jpg",
                cameraId = cameraId,
                empName = empName,
                empDpt = empDeptName,
                registration = "Yes"
            };
            string messageString = JsonConvert.SerializeObject(data);
            Message message = new Message(Encoding.ASCII.GetBytes(messageString));

            await UploadImageToBlobAsync($"{data.blobName}", faceImage);
            Debug.WriteLine("Image uploaded.");

            await _deviceClient.SendEventAsync(message);
            Debug.WriteLine("Message sent.");

            faceImage.Dispose();

        }
        /// <summary>
        /// Upload image in blob
        /// </summary>
        /// <param name="blobName"></param>
        /// <param name="imageStream"></param>
        /// <returns></returns>
        private async Task UploadImageToBlobAsync(string blobName, Stream imageStream)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_configuration.StorageConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(_configuration.StorageContainer);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);
            imageStream.Seek(0, SeekOrigin.Begin);
            await blockBlob.UploadFromStreamAsync(imageStream);
        }
    }
}