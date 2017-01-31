﻿using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SmartMeetingRoom.CaptureApp
{
    internal class DeviceConfiguration : IDeviceConfiguration
    {
        private readonly IConfiguration _configuration;

        public DeviceConfiguration(EasClientDeviceInformation clientDeviceInformation, IConfiguration configuration)
        {
            DeviceId = clientDeviceInformation.Id;
            _deviceKey = null;
            //_deviceKey = "WG6ibk0O1OYw+lIJRpwkK9w4ZW5NQu4BagR1aNtsY2U=";
            _configuration = configuration;
            _lockObject = new object();
        }

        public Guid DeviceId { get; }
        // public string DeviceIdString => "CAMERADEVICE";
        public string DeviceIdString => DeviceId.ToString();
        private string _deviceKey;

        private readonly object _lockObject;

        public string DeviceKey
        {
            get
            {
                if (string.IsNullOrEmpty(_deviceKey))
                {
                    lock (_lockObject)
                    {
                        if (string.IsNullOrEmpty(_deviceKey))
                        {
                            _deviceKey = "2uhUYY07LDtnSyHCJv+1G9QiJzstjk74qg73GWdvsi4=";
                        }
                    }
                }

                return _deviceKey;
            }
        }

        private static async Task<string> RegisterDeviceAsync(IConfiguration configuration, string deviceId)
        {
            string deviceKey;
            var regPayload = new
            {
                DeviceId = deviceId,
                MeetingRoomId = "B776464F-5917-41FD-BD0C-417913645EF2",
                Confidence = 0.7
            };

            HttpClient httpClient = new HttpClient();
            HttpResponseMessage regResult =
                await
                    httpClient.PostAsync(configuration.BackendUrl + "/api/device",
                        new StringContent(JsonConvert.SerializeObject(regPayload), Encoding.UTF8, "application/json"));

            if (regResult.IsSuccessStatusCode)
            {
                JObject regResponse = JObject.Parse(await regResult.Content.ReadAsStringAsync());
                deviceKey = regResponse["deviceKey"].ToString();
            }
            else
            {
                string errorMessage = "Device registration failed. " + await regResult.Content.ReadAsStringAsync();
                Debug.WriteLine(errorMessage);
                throw new Exception(errorMessage);
            }
            return deviceKey;
        }
    }
}