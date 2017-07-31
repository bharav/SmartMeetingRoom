using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.Builders;
using SmartMeetingRoom.Common.DTO;



namespace SmartMeetingRoom.Common.Services
{
    public class MongoDBService
    {

        private IMongoDatabase _database { get; }
        internal IMongoCollection<Device> DeviceCollection { get; }
        internal IMongoCollection<Employee> EmployeeCollection { get; }
        internal IMongoCollection<Calender> CalenderCollection { get; }

        public MongoDBService(IAppConfiguration config)
        {
            try
            {
                MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(config.MongoDBConnectionString));
                //if (config.IsMongoSSL)
                //{
                    settings.SslSettings = new SslSettings { EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12 };
                //}
                var mongoClient = new MongoClient(settings);
                _database = mongoClient.GetDatabase(config.MongoDBDatabaseName);
                DeviceCollection = _database.GetCollection<Device>("devices");
                EmployeeCollection = _database.GetCollection<Employee>("employees");
                CalenderCollection = _database.GetCollection<Calender>("calenders");
            }
            catch (Exception ex)
            {
                throw new Exception("Can not access to db server.", ex);
            }

        }
        public Device GetDevice(string deviceId)
        {
            IEnumerable<Device> deviceQuery = DeviceCollection.Find(d => d.DeviceId == deviceId).ToEnumerable();
            Device selectedDevice = deviceQuery.FirstOrDefault();
            return selectedDevice;
        }
        public async Task<List<Device>> GetAllDevices()
        {
            List<Device> devices = await DeviceCollection.Find(new BsonDocument()).ToListAsync();
            return devices;
        }
        public Calender GetMeetingDetails(string calenderId)
        {
            var id = new ObjectId(calenderId);
            IEnumerable<Calender> calenderQuery = CalenderCollection.Find(d=> d.Id == id).ToEnumerable();
            Calender selectedCalender = calenderQuery.FirstOrDefault();
            return selectedCalender;
        }

        public async Task<Employee> GetEmployeeById(string Id)
        {
            var id = new ObjectId(Id);
            IEnumerable<Employee> employeeQuery = await EmployeeCollection.Find(d => d.Id == id).ToListAsync();
            Employee selectedEmployee = employeeQuery.FirstOrDefault();
            return selectedEmployee;
        }
        public Employee GetEmployeeByPersonId(string PersonId)
        {
            IEnumerable<Employee> employeeQuery = EmployeeCollection.Find(d => d.PersonId == PersonId).ToEnumerable();
            Employee selectedEmployee = employeeQuery.FirstOrDefault();
            return selectedEmployee;
        }
        /// <summary>
        /// Get Latest Calender 
        /// </summary>
        /// <returns></returns>
        public async Task<Calender> LatestCalender()
        {
            List<Calender> calenders=await CalenderCollection.Find(new BsonDocument()).ToListAsync();
            return calenders.First();
        }

        /// <summary>
        /// Get Calender By Calender Id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<Calender> GetCalenderById(string Id)
        {
            var id = new ObjectId(Id);
            IEnumerable<Calender> calenderQuery = await CalenderCollection.Find(d => d.Id == id).ToListAsync();
            Calender selectedCalender = calenderQuery.FirstOrDefault();
            return selectedCalender;

        }

        /// <summary>
        /// Get Calender By Calender Id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<List<Calender>> GeetCalenderByMeedingRoomId(string MeetingRoomId)
        {
            
            List<Calender> calenders = await CalenderCollection.Find(d => d.MeetingRoomId == MeetingRoomId).ToListAsync();
            return calenders;
        }

        /// <summary>
        /// Get All Calender
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<List<Calender>> GetAllCalender()
        {
            List<Calender> calenders = await CalenderCollection.Find(new BsonDocument()).ToListAsync();
            return calenders;
        }

        public async Task<bool> SaveAsync(Employee employee)
        {
            if (GetEmployeeByPersonId(employee.PersonId) == null)
            {
                await EmployeeCollection.InsertOneAsync(employee);
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task SaveAsync(Device device)
        {
            if (GetDevice(device.id.ToString()) == null)
            {
                await DeviceCollection.InsertOneAsync(device);
            }

        }
        public async Task SaveAsync(Calender _calender)
        {
            await CalenderCollection.InsertOneAsync(_calender);
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
            IEnumerable<Calender> calenderQuery = CalenderCollection.Find(d => d.MeetingRoomId == meetingRoomId).ToEnumerable();
            Calender exsiting = calenderQuery.FirstOrDefault();
            if (exsiting.MeetingAttendees != null)
            {
                List<MeetingAttendee> meetingattendees = exsiting.MeetingAttendees.ToList();
                var personquery = meetingattendees.Where(p => p.PersonId == attendee.PersonId);
                if (personquery.Count() <= 0)
                {
                    meetingattendees.Add(attendee);
                    exsiting.MeetingAttendees = meetingattendees.ToArray();
                    //Update Calender Item
                    await CalenderCollection.ReplaceOneAsync(m => m.Id == exsiting.Id, exsiting);

                }
            }
            else
            {
                List<MeetingAttendee> meetingattendees = new List<MeetingAttendee>();
                meetingattendees.Add(attendee);
                exsiting.MeetingAttendees = meetingattendees.ToArray();
                await CalenderCollection.ReplaceOneAsync(m => m.Id == exsiting.Id, exsiting);
            }

        }

    }
}
