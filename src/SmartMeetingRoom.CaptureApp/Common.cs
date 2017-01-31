using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Devices.Enumeration;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.FaceAnalysis;
using Windows.Media.MediaProperties;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.Storage.Streams;
using Microsoft.Practices.Unity;
using Windows.Storage;
using System.IO;

namespace SmartMeetingRoom.CaptureApp
{
    public class Common
    {

        internal static async Task<UnityContainer> InitializeDiContainer()
        {
            UnityContainer container = new UnityContainer();

            container.RegisterType<IConfiguration, StaticConfiguration>();
            container.RegisterType<IDeviceConfiguration, DeviceConfiguration>();
            container.RegisterInstance(typeof(EasClientDeviceInformation), new EasClientDeviceInformation());
            container.RegisterType<IImagePersiter, AzureImagePersister>();
            container.RegisterType<IImageFilter, LocalFaceDetector>();
            container.RegisterInstance(typeof(FaceDetector),
                FaceDetector.IsSupported ? await FaceDetector.CreateAsync() : null);

            return container;
        }

        private static async Task SetMaxResolution(MediaCapture mediaCapture)
        {
            IReadOnlyList<IMediaEncodingProperties> res =
                mediaCapture.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.VideoPreview);
            uint maxResolution = 0;
            int indexMaxResolution = 0;

            if (res.Count >= 1)
            {
                for (int i = 0; i < res.Count; i++)
                {
                    VideoEncodingProperties vp = (VideoEncodingProperties)res[i];

                    if (vp.Width <= maxResolution) continue;
                    indexMaxResolution = i;
                    maxResolution = vp.Width;
                }
                await
                    mediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview,
                        res[indexMaxResolution]);
            }
        }

        internal static async Task<MediaCapture> InitializeCameraAsync()
        {
            MediaCapture mediaCaptureDevices = new MediaCapture();
            DeviceInformationCollection videoDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

            if (!videoDevices.Any())
            {
                Debug.WriteLine("No cameras found.");
                return null;
            }
            else
            {
                MediaCapture mediaCapture = new MediaCapture();
                MediaCaptureInitializationSettings mediaInitSettings = new MediaCaptureInitializationSettings
                {
                    VideoDeviceId = videoDevices[0].Id
                };
                await mediaCapture.InitializeAsync(mediaInitSettings);
                await SetMaxResolution(mediaCapture);
                return mediaCapture;
            }

        }

        private static async Task<Tuple<BitmapDecoder, IRandomAccessStream>> GetPhotoStreamAsync(
            MediaCapture mediaCapture)
        {
            InMemoryRandomAccessStream photoStream = new InMemoryRandomAccessStream();
            await mediaCapture.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), photoStream);
            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(photoStream);
            return new Tuple<BitmapDecoder, IRandomAccessStream>(decoder, photoStream.CloneStream());
        }

        internal async Task Capture(MediaCapture cameraDevice, IImageFilter filter, string empName, string empDeptName)
        {
            // IEnumerable<MediaCapture> cameraDevices = devices as MediaCapture[] ?? devices.ToArray();
            while (true)
            {
                string cameraId = cameraDevice.MediaCaptureSettings.VideoDeviceId;
                Debug.WriteLine($"Processing camera: {cameraId}");
                BitmapDecoder bitmapDecoder;
                IRandomAccessStream imageStream;
                try
                {
                    Tuple<BitmapDecoder, IRandomAccessStream> photoData = await GetPhotoStreamAsync(cameraDevice);
                    bitmapDecoder = photoData.Item1;
                    imageStream = photoData.Item2;

                    Debug.WriteLine($"Got stream from camera: {cameraId}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Camera {cameraId} failed with message: {ex.Message}");
                    continue;
                }
#pragma warning disable 4014
                filter?.ProcessImageAsync(bitmapDecoder, imageStream, cameraId);
#pragma warning restore 4014

            }
            // ReSharper disable once FunctionNeverReturns
        }
        /// <summary>
        /// Asynchronously begins live webcam feed
        /// </summary>
        public async Task StartCameraPreview(MediaCapture mediaCapture)
        {
            try
            {
                await mediaCapture.StartPreviewAsync();
            }
            catch
            {
                Debug.WriteLine("Failed to start camera preview stream");

            }
        }

        /// <summary>
        /// Asynchronously ends live webcam feed
        /// </summary>
        public async Task StopCameraPreview(MediaCapture mediaCapture)
        {
            try
            {
                await mediaCapture.StopPreviewAsync();
            }
            catch
            {
                Debug.WriteLine("Failed to stop camera preview stream");
            }
        }
        internal async Task CapturePhoto(MediaCapture mediaCapture, IImageFilter filter, string empName, string empDeptName)
        {
            string cameraId = mediaCapture.MediaCaptureSettings.VideoDeviceId;
            // Create storage file in local app storage
            string fileName = GenerateNewFileName() + ".jpg";
            CreationCollisionOption collisionOption = CreationCollisionOption.GenerateUniqueName;
            StorageFile file = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(fileName, collisionOption);

            // Captures and stores new Jpeg image file
            await mediaCapture.CapturePhotoToStorageFileAsync(ImageEncodingProperties.CreateJpeg(), file);
            Stream filestream = await file.OpenStreamForReadAsync();
            //  Stream faceImage, string cameraId, string empName, string empDeptName
            // Return image file
#pragma warning disable 4014
            filter?.ProcessImageAsync(filestream, cameraId, empName, empDeptName);
#pragma warning restore 4014

        }

        /// <summary>
        /// Generates unique file name based on current time and date. Returns value as string.
        /// </summary>
        private string GenerateNewFileName()
        {
            return DateTime.UtcNow.ToString("HH-mm-ss") + "_Employee";
        }
    }
}
