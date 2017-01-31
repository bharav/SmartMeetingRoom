using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using SmartMeetingRoom.Common.DTO;

namespace SmartMeetingRoom.Common.Services
{
    public class DocumentDbService
    {
        public DocumentClient DocumentClient { get; }

        internal DocumentCollection DevicesCollection { get; }

        internal DocumentCollection EmployeesCollection { get; }

        public DocumentCollection CalendersCollection { get; }

        public DocumentDbService(IAppConfiguration Config)
        {

            DocumentClient = new DocumentClient(new Uri(Config.Endpoint), Config.Key);
            Database database = DocumentClient.CreateDatabaseQuery().Where(db => db.Id == "smartmeeting").AsEnumerable().First();
            DevicesCollection =
                DocumentClient.CreateDocumentCollectionQuery(database.SelfLink)
                    .Where(c => c.Id == "devices")
                    .AsEnumerable()
                    .First();
            EmployeesCollection =
                DocumentClient.CreateDocumentCollectionQuery(database.SelfLink)
                    .Where(c => c.Id == "employees")
                    .AsEnumerable()
                    .First();
            CalendersCollection =
                DocumentClient.CreateDocumentCollectionQuery(database.SelfLink)
                    .Where(c => c.Id == "calenders")
                    .AsEnumerable()
                    .First();
        }

        public Device GetDevice(string deviceId)
        {
            IEnumerable<Device> deviceQuery = DocumentClient
                .CreateDocumentQuery<Device>(DevicesCollection.DocumentsLink)
                .Where(d => d.DeviceId == deviceId)
                .AsEnumerable();

            Device selectedDevice = deviceQuery.FirstOrDefault();

            return selectedDevice;
        }

        public async Task<List<Device>> GetAllDevices()
        {
            var sql = new SqlQuerySpec
            {
                QueryText = "SELECT * FROM c",
            };
            IDocumentQuery<dynamic> query = DocumentClient.CreateDocumentQuery(DevicesCollection.DocumentsLink, sql).AsDocumentQuery();
            List<Device> devices = new List<Device>();
            foreach (dynamic device in await query.ExecuteNextAsync())
            {
                devices.Add(device);
            }
            return devices;
        }

        public Calender GetMeetingDetails(string calenderId)
        {
            IEnumerable<Calender> calenderQuery = DocumentClient
                 .CreateDocumentQuery<Calender>(CalendersCollection.DocumentsLink)
                 .Where(d => d.Id == calenderId)
                 .AsEnumerable();

            Calender selectedCalender = calenderQuery.FirstOrDefault();
            return selectedCalender;
        }
        public async Task<Employee> GetEmployeeById(string Id)
        {

            var sql = new SqlQuerySpec
            {
                QueryText = "SELECT * FROM c WHERE c.id = @id",
                Parameters = new
             SqlParameterCollection {
               new SqlParameter {
                  Name = "@id", Value = Id
               }
            }
            };

            IDocumentQuery<dynamic> query =
                DocumentClient.CreateDocumentQuery(EmployeesCollection.DocumentsLink, sql).AsDocumentQuery();

            var documents = await query.ExecuteNextAsync();
            return documents.First();
        }
        public Employee GetEmployeeByPersonId(string PersonId)
        {

            IEnumerable<Employee> employeeQuery = DocumentClient
               .CreateDocumentQuery<Employee>(EmployeesCollection.DocumentsLink)
               .Where(d => d.PersonId == PersonId)
               .AsEnumerable();

            Employee selectedEmployee = employeeQuery.FirstOrDefault();
            return selectedEmployee;
        }

        public async Task SaveAsync(Calender _calender)
        {
            await DocumentClient.CreateDocumentAsync(CalendersCollection.DocumentsLink, _calender);
        }

        public async Task<bool> SaveAsync(Employee employee)
        {
            if (GetEmployeeByPersonId(employee.PersonId) == null)
            {
                await DocumentClient.CreateDocumentAsync(EmployeesCollection.DocumentsLink, employee);
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task SaveAsync(Device device)
        {
            if (GetDevice(device.id) == null)
            {
                await DocumentClient.CreateDocumentAsync(DevicesCollection.DocumentsLink, device);
            }

        }

        /// <summary>
        /// Get Latest Calender 
        /// </summary>
        /// <returns></returns>
        public async Task<Calender> LatestCalender()
        {
            var sql = new SqlQuerySpec
            {
                QueryText = "SELECT * FROM c",
            };

            IDocumentQuery<dynamic> query =
                DocumentClient.CreateDocumentQuery(CalendersCollection.DocumentsLink, sql).AsDocumentQuery();
            var documents = await query.ExecuteNextAsync();
            Calender cal = (Calender)documents.First();
            return documents.First();
        }

        /// <summary>
        /// Get Calender By Calender Id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<Calender> GetCalenderById(string Id)
        {
            var sql = new SqlQuerySpec
            {
                QueryText = "SELECT * FROM c WHERE c.Id = @id",
                Parameters = new
         SqlParameterCollection {
               new SqlParameter {
                  Name = "@id", Value = Id
               }
            }
            };

            IDocumentQuery<dynamic> query =
                DocumentClient.CreateDocumentQuery(CalendersCollection.DocumentsLink, sql).AsDocumentQuery();

            var documents = await query.ExecuteNextAsync();
            return documents.First();

        }

        /// <summary>
        /// Get Calender By Calender Id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<List<Calender>> GeetCalenderByMeedingRoomId(string MeetingRoomId)
        {
            var sql = new SqlQuerySpec
            {
                QueryText = "SELECT * FROM c WHERE c.MeetingRoomId = @meetingId",
                Parameters = new
         SqlParameterCollection {
               new SqlParameter {
                  Name = "@meetingId", Value = MeetingRoomId
               }
            }
            };

            IDocumentQuery<dynamic> query =
                DocumentClient.CreateDocumentQuery(CalendersCollection.DocumentsLink, sql).AsDocumentQuery();

            List<Calender> calenders = new List<Calender>();
            foreach (dynamic calender in await query.ExecuteNextAsync())
            {
                calenders.Add(calender);
            }
            return calenders;
        }

        /// <summary>
        /// Get All Calender
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<List<Calender>> GetAllCalender()
        {
            var sql = new SqlQuerySpec
            {
                QueryText = "SELECT * FROM c",
            };

            IDocumentQuery<dynamic> query =
                DocumentClient.CreateDocumentQuery(CalendersCollection.DocumentsLink, sql).AsDocumentQuery();

            List<Calender> calenders = new List<Calender>();
            foreach (dynamic calender in await query.ExecuteNextAsync())
            {
                calenders.Add((Calender)calender);
            }
            return calenders;
        }
        /// <summary>
        /// Update Attendee in the calender object
        /// </summary>
        /// <param name="attendee"></param>
        /// <param name="meetingRoomId"></param>
        /// <param name="eventDateTime"></param>
        /// <returns></returns>
        public async Task UpdateAttendeeInCalender(MeetingAttendee attendee, string meetingRoomId, DateTime eventDateTime)
        {

            var sql = new SqlQuerySpec
            {
                QueryText = "SELECT * FROM c WHERE c.MeetingRoomId = @meetingRoomId",// and c.StartTime=<@evenDateTime=<c.EndTime",
                Parameters = new
                SqlParameterCollection {
                       new SqlParameter {Name = "@meetingRoomId", Value = meetingRoomId},
                       new SqlParameter {Name="@evenDateTime", Value= eventDateTime }
                       }
            };
            //Get the calender Item
            IDocumentQuery<dynamic> query =
               DocumentClient.CreateDocumentQuery(CalendersCollection.DocumentsLink, sql).AsDocumentQuery();

            var documents = await query.ExecuteNextAsync();
            Document document = documents.First();
            Calender exsiting = documents.First();
            List<MeetingAttendee> meetingattendees = exsiting.MeetingAttendees.ToList();
            var personquery = meetingattendees.Where(p => p.PersonId == attendee.PersonId);
            if (personquery.Count() <= 0) {
                meetingattendees.Add(attendee);
                exsiting.MeetingAttendees = meetingattendees.ToArray();
                //Update Calender Item
                await DocumentClient.ReplaceDocumentAsync(document.SelfLink, exsiting);

            }

        }


    }
}