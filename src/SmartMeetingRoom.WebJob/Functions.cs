using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using SmartMeetingRoom.Common.Services;
using SmartMeetingRoom.Common;
using SmartMeetingRoom.Common.DTO;
using System.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System.Diagnostics;
using Microsoft.ServiceBus.Messaging;

namespace SmartMeetingRoom.WebJob
{
    public class Functions
    {

        private const string personGroupId = "emcappemployee";

        private static MongoDBService ddbService;
        private static FaceApiService faceApiService;
        private static StorageService storageService;

        /// <summary>
        /// Initializing Services
        /// </summary>
        private static void InitializeServices()
        {
            IAppConfiguration config = new AppConfiguration();
            ddbService = new MongoDBService(config);
            faceApiService = new FaceApiService(ConfigurationManager.AppSettings["FaceApiSubscriptionKey"]);
            storageService =
                new StorageService(ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ConnectionString,
                    "faces");
        }
        // This function will get triggered/executed when a new message is written 
        // on an Azure Queue called queue.
        public static async void ProcessQueueMessage([ServiceBusTrigger("event_queue")] BrokeredMessage message, TextWriter log)
        {
           // message.Complete();
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
            if (registration)
            {
                await storageService.DeleteBlockBlob($"{blobName}");
            }
            Debug.WriteLine("Processing done.");
        }
    }
}
