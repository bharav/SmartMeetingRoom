Introduction
============

**Smart Meeting Room** is an IoT proof of concept using Camera enabled devices to send data to cloud which in turn will help backend analytical skills like Face Analytics to predict the face captured from the devices.
This solution is using Azure as a cloud infrastructure. This solution uses below  Azure services:

1. Azure IoT Hub
2. Azure Storage
3. Azure Service Bus
4. Azure DocumentDB
5. Azure Web Apps with Web Jobs
6. Microsoft Cognitive Services(Face Api and Emotion Api)

##Setting up Azure Services##

Azure Resource Manager is used to setup **Azure Services**. Download SmartMeetingRoom solution from the repositiory and Use **SmartMeetingRoom.Azure** project to create all the services. This project include below given files 
* Deploy-AzureResourceGroup.ps1 
* parameters.json 
* template.json

To create project right click on the project and click **Deploy**

![1.png](C:/Users/bharav4/Pictures/Smartmeeting/1.png "Deploy Azure Services")

All services will be created on deployment the above project. Your Azure protal will look like below 

![2.PNG](C:/Users/bharav4/Pictures/Smartmeeting/2.PNG "Azure Services")



##Document DB and Cognitive Service Initialization##

Document Collections and faceapi person group needs to be created for future use. **SmartMeetingRoom.Initializer** will be used for all the ititalization work. Put all the configuartion value from the above created services in "app.config" file. Run this project after changes has been done. It will create below collection in document DB:

* Calaender
* Employee
* Device


## Solution Architecture and Flow ##

Flow and architecture diagram for this solution is given below

![3.png](C:/Users/bharav4/Pictures/Smartmeeting/3.png "Solution Architecture")

**Note**: Xamarin application is a not a part of this solution 

##IoT Device Application##

Windows IoT Core is used with Raspberry Pi 3 is used for this POC. **SmartMeetingRoom.CaptureApp** is UWP app which will take face images form the in memory streams and sent in two process

1. Face images to Azure storage 
2. Messages to IoT Hub

Below is the code snippet for capturing images from in memory stream:

```cs
 public async Task ProcessImageAsync(BitmapDecoder bitmapDecoder, IRandomAccessStream imageStream,
            string cameraId)
        {
            try
            {
                SoftwareBitmap image =
                    await
                        bitmapDecoder.GetSoftwareBitmapAsync(bitmapDecoder.BitmapPixelFormat,
                            BitmapAlphaMode.Premultiplied);

                const BitmapPixelFormat faceDetectionPixelFormat = BitmapPixelFormat.Gray8;
                if (image.BitmapPixelFormat != faceDetectionPixelFormat)
                {
                    image = SoftwareBitmap.Convert(image, faceDetectionPixelFormat);
                }
                IEnumerable<DetectedFace> detectedFaces = await _faceDetector.DetectFacesAsync(image);

                if (detectedFaces!=null)
                {
                    List<Stream> faceImages = new List<Stream>();
                    foreach (DetectedFace face in detectedFaces)
                    {
                        MemoryStream faceImageStream = new MemoryStream();
                        Image faceImage = new Image(imageStream.AsStreamForRead());
                        int width, height, xStartPosition, yStartPosition;
                        EnlargeFaceBoxSize(face, image, out width, out height, out xStartPosition,
                            out yStartPosition);
                        faceImage.Crop(width, height,
                            new Rectangle(xStartPosition, yStartPosition,
                                width, height)).SaveAsJpeg(faceImageStream, 80);
                        faceImages.Add(faceImageStream);
                    }


                    await _imagePersiter.PersistAsync(faceImages, cameraId);
                }
            }
            catch (Exception ex)
            {
                //ToDo Logging
                Debug.WriteLine(ex.Message);
            }
        }
```

After processing face image it is sent to Iot Hub and Azure Storage

```cs
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
```

This project used PIR sensor for switching on the camera as soon as any one enter meeting room. Below is the code to initialize raspberry pi's gpio pin 

```cs
 public bool Initialize()
        {
            // Get the GpioController
            gpioController = GpioController.GetDefault();
            if (gpioController == null)
            {
                // There is no Gpio Controller on this device so return false.
                return false;
            }

            // Open the GPIO pin that interacts with the PIR sensor
            pirSensor = gpioController.OpenPin(GpioConstants.PirPin);

            if (pirSensor == null)
            {
                // Pin wasn't opened properly so return false
                return false;
            }

            // Set the direction of the PIR sensor as input
            pirSensor.SetDriveMode(GpioPinDriveMode.Input);

            //Initialization was successfull, return true
            return true;
        }

        /// <summary>
        /// Returns the GpioPin for the PIR sensor. Will be used in to setup event handler when motion is detected.
        /// </summary>
        public GpioPin GetPirSensor()
        {
            return pirSensor;
        }
        public static class GpioConstants
        {
            // The GPIO pin that the PIR motion sensor is attached to
            public const int PirPin = 5;
        }
``` 

##Processing messages from Service Bus##

Azure WebJob is created for processing message from service bus message queue. Below is the code for processing messages 
```cs

    public class Functions
    {

        private const string personGroupId = "emcappemployee";

        private static DocumentDbService ddbService;
        private static FaceApiService faceApiService;
        private static StorageService storageService;

        /// <summary>
        /// Initializing Services
        /// </summary>
        private static void InitializeServices()
        {
            IAppConfiguration config = new AppConfiguration();
            ddbService = new DocumentDbService(config);
            faceApiService = new FaceApiService(ConfigurationManager.AppSettings["FaceApiSubscriptionKey"]);
            storageService =
                new StorageService(ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ConnectionString,
                    "faces");
        }
        // This function will get triggered/executed when a new message is written 
        // on an Azure Queue called queue.
        public static async void ProcessQueueMessage([ServiceBusTrigger("event_queue")] BrokeredMessage message, TextWriter log)
        {

            InitializeServices();
            Debug.Write(message);
            log.WriteLine(message);
            //await message.CompleteAsync();
            string s = new StreamReader(message.GetBody<Stream>(), Encoding.UTF8).ReadToEnd();
            JObject mess = JObject.Parse(s);
            string deviceId = mess["deviceId"].ToString();
            //JToken deviceName = mess["IoTHub"]["ConnectionDeviceId"];
            JToken blobName = mess["blobName"];
            string cameraId = mess["cameraId"].ToString().ToLower();
            bool registration = false;
            if (mess["registration"].ToString() == "Yes")
            {
                registration = true;
            }
            else {
                registration = false;
            }
            string empName;
            string empDptName;
            //Value will be passed in IOT Hub only when it was sent from Registration App
            if (mess["empName"] != null)
            {
                empName = mess["empName"].ToString();
            }
            else
            {
                empName = "";
            }
            if (mess["empDpt"] != null)
            {
                empDptName = mess["empDpt"].ToString();
            }
            else
            {
                empDptName = "";
            }

            Device device = ddbService.GetDevice(deviceId);
            string meetingRoomId = device.MeetingRoomId;
            bool blNewEmployeeAdded = false;

            MemoryStream faceStream = new MemoryStream();
            string blobContainer;
            if (registration)
            {
                blobContainer = $"{blobName}";
            }
            else
            {
                blobContainer = $"{blobName}";
            }
            //Download Image from Blob
            faceStream = storageService.DownloadBlockBlob(blobContainer);
            MemoryStream detectStream = new MemoryStream();
            faceStream.CopyTo(detectStream);
            detectStream.Seek(0, SeekOrigin.Begin);
            MeetingAttendee attendee = new MeetingAttendee();
            Face[] detectionResult =
                        await
                            faceApiService.DetectAsync(detectStream,
                                returnFaceAttributes:
                                new List<FaceAttributeType>
                                {
                            FaceAttributeType.Age,
                            FaceAttributeType.Gender,
                            FaceAttributeType.Smile
                                });
            if (detectionResult.Length > 0)
            {
                if (detectionResult[0].FaceAttributes.Smile > 0.5) {
                    attendee.Smile = "Smile";
                }
                else
                {
                    attendee.Smile = "Sad";
                }
                attendee.Gender = detectionResult[0].FaceAttributes.Gender;
            }

            Debug.WriteLine($"Detecting done. Got {detectionResult.Count()} faces. Image: {blobName}");
            string personId = "";
            IdentifyResult[] identifyResults =
            await faceApiService.IdentifyAsync(personGroupId, detectionResult.Select(f => f.FaceId).ToArray());
            Debug.WriteLine($"Identification done. Got {identifyResults.Count()} results.");

            if (!identifyResults.Any() || !identifyResults.FirstOrDefault().Candidates.Any())
            {
                Debug.WriteLine("Unable to identify person for this face. Creating new person.");
                CreatePersonResult persResult =
                    await faceApiService.CreatePersonAsync(personGroupId, Guid.NewGuid().ToString());
                Debug.WriteLine($"New person created with PersonId: {persResult.PersonId}");
                personId = persResult.PersonId.ToString();
                if (registration)
                {
                    Employee emp = new Employee();
                    emp.PersonId = personId;
                    emp.EmpName = empName;
                    emp.Department = empDptName;
                    emp.BlobName = blobName.ToString();
                    blNewEmployeeAdded = await ddbService.SaveAsync(emp);
                }
                else
                {
                    attendee.PersonId = personId;
                    attendee.EmpName = "NaN";
                    attendee.Department = "NaN";
                    attendee.BlobName = blobName.ToString();

                    await ddbService.UpdateAttendeeInCalender(attendee, meetingRoomId, DateTime.Now);
                }
            }
            else
            {
                Candidate candidate = identifyResults.FirstOrDefault().Candidates.FirstOrDefault();
                if (candidate.Confidence > 0.5)
                {
                    Person pers = await faceApiService.GetPersonAsync(personGroupId, candidate.PersonId);

                    Debug.WriteLine(
                        $"Person recognized: {pers.PersonId}. We have {pers.PersistedFaceIds.Length} faces recorded for this person.");

                    if (pers.PersistedFaceIds.Length == 248)
                    {
                        Guid persistedFaceId = pers.PersistedFaceIds.OrderBy(x => Guid.NewGuid()).FirstOrDefault();
                        await faceApiService.DeletePersonFaceAsync(personGroupId, candidate.PersonId, persistedFaceId);
                    }
                    if (registration)
                    {
                        Employee employee = new Employee();
                        employee.PersonId = pers.PersonId.ToString();
                        employee.EmpName = empName;
                        employee.Department = empDptName;
                        employee.BlobName = blobName.ToString();
                        blNewEmployeeAdded = await ddbService.SaveAsync(employee);
                    }
                    else
                    {
                        Employee emp = ddbService.GetEmployeeByPersonId(candidate.PersonId.ToString());
                        if (emp != null)
                        {
                            attendee.PersonId = pers.PersonId.ToString();
                            attendee.EmpName = emp.EmpName;
                            attendee.Department = emp.Department;
                            attendee.BlobName = emp.BlobName;
                        }
                        await ddbService.UpdateAttendeeInCalender(attendee, meetingRoomId, DateTime.Now);
                    }
                }
            }

            MemoryStream addFaceStream = new MemoryStream();
            faceStream.Seek(0, SeekOrigin.Begin);
            faceStream.CopyTo(addFaceStream);
            addFaceStream.Seek(0, SeekOrigin.Begin);
            if (!string.IsNullOrEmpty(personId))
            {
                await faceApiService.AddPersonFaceAsync(personGroupId, new Guid(personId), addFaceStream);
                await faceApiService.TrainPersonGroupAsync(personGroupId);
            }
            faceStream.Dispose();
            if (!registration)
            {
                await storageService.DeleteBlockBlob($"{blobName}");
            }
            Debug.WriteLine("Processing done.");
        }
    }
```

To Deploy above project as webjob right click on project and select "Publish as webjob". 
![4.png](C:/Users/bharav4/Pictures/Smartmeeting/4.png "WebJob")

 
##Common Services##

Few common services has been written in "SmartMeetingRoom.Common" project. These services are used both by webjob and web api. These services are 

* DocumentDBService
* FaceApiService   
* IoTHubService
* StorageService


These services are injected as an when required using **dependency injection**.

##Web API for consuming data from front End##

ASP.Net Core API has been created for consuming data from documentDB and azure storage. These api can be called without any authentication using REST API "Get" verb. To configure the connection strings and Azure storage appsettings.json file needs to edited in **SmartMeetingRoom.Backend** project file. 

To deploy this project as azure webapp right click on project and click **Publish** 
![5.png](C:/Users/bharav4/Pictures/Smartmeeting/5.png "Publish")



















 