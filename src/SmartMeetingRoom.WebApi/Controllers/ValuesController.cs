using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SmartMeetingRoom.Common.DTO;
using SmartMeetingRoom.Common.Services;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
namespace SmartMeetingRoom.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private DocumentDbServiceCalender _ddbService;

        public ValuesController()
        {
            _ddbService = new DocumentDbServiceCalender();
        }
        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            try
            {
                _ddbService.GetMeetingDetails("5976e2ab3e168fd182b5f238");
                return new string[] { "value1", "value2" };
            }
            catch (Exception ex) {
              return new string[] { "value1", "value2" };
            }
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }

    public class Calender
    {
        private DateTime created = DateTime.Now;
        public ObjectId Id { get; set; }
        [BsonElement("MeetingRoomId")]
        public string MeetingRoomId { get; set; }
        [BsonElement("StartTime")]
        public DateTime StartTime { get; set; }
        [BsonElement("EndTime")]
        public DateTime EndTime { get; set; }
        [BsonElement("Invitees")]
        public MeetingInvitee[] Invitees { get; set; }
        [BsonElement("MeetingAttendees")]
        public MeetingAttendee[] MeetingAttendees { get; set; }
        [BsonElement("Confidence")]
        public double Confidence { get; set; }
        [BsonElement("Created")]
        public DateTime Created
        {
            get { return created; }
            set
            {
                if (value == null) created = DateTime.Now; else created = value;
            }
        }

    }

    public class MeetingInvitee
    {
        [BsonElement("PersonId")]
        public string PersonId { get; set; }
        [BsonElement("EmpName")]
        public string EmpName { get; set; }
        [BsonElement("Department")]
        public string Department { get; set; }
        [BsonElement("BlobName")]
        public string BlobName { get; set; }
    }

    public class MeetingAttendee
    {
        [BsonElement("EmpName")]
        public string EmpName { get; set; }
        [BsonElement("Department")]
        public string Department { get; set; }
        [BsonElement("BlobName")]
        public string BlobName { get; set; }
        [BsonElement("PersonId")]
        public string PersonId { get; set; }
        [BsonElement("TimeEntered")]
        public DateTime TimeEntered { get; set; }
        [BsonElement("Smile")]
        public string Smile { get; set; }
        [BsonElement("Gender")]
        public string Gender { get; set; }
    }
    public class DocumentDbServiceCalender
    {
        private IMongoDatabase _database { get; }
        internal IMongoCollection<Calender> CalenderCollection { get; }

        public DocumentDbServiceCalender()
        {
            try
            {
                MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl("mongodb://localhost:27017"));
                var mongoClient = new MongoClient(settings);
                _database = mongoClient.GetDatabase("smartmeeting");
                CalenderCollection = _database.GetCollection<Calender>("calenders");
            }
            catch (Exception ex)
            {
                throw new Exception("Can not access to db server.", ex);
            }

        }


        public async Task SaveAsync(Calender _calender)
        {

            await CalenderCollection.InsertOneAsync(_calender);
        }
        public Calender GetMeetingDetails(string calenderId)
        {
            var id = new ObjectId(calenderId);
            IEnumerable<Calender> calenderQuery = CalenderCollection.Find(d => d.Id == id).ToList(); ;
            Calender selectedCalender = calenderQuery.FirstOrDefault();
            return selectedCalender;
        }

        /// <summary>
        /// Get Latest Calender 
        /// </summary>
        /// <returns></returns>
        public async Task<Calender> LatestCalender()
        {
            List<Calender> calenders = await CalenderCollection.Find(new BsonDocument()).ToListAsync();
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

    }
}
