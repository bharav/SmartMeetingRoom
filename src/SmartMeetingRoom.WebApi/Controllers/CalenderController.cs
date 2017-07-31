using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SmartMeetingRoom.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class CalenderController : Controller
    {
        private DocumentDbServiceCalender _ddbService;

        public CalenderController()
        {
            _ddbService = new DocumentDbServiceCalender();
        }
        // GET: api/values
        [HttpGet]
        public async Task<IActionResult> GetAllCalender()
        {
            List<Calender> calenders = await _ddbService.GetAllCalender();
            HttpContext.Response.StatusCode = 201;
            return Json(calenders);
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCalenderById(string id)
        {
            Calender calender = await _ddbService.GetCalenderById(id);
            HttpContext.Response.StatusCode = 201;
            return Json(calender);
        }

        // POST api/values
        [HttpPost]
        public async Task<IActionResult> CreateCalender([FromBody]Calender cal)
        {
            await _ddbService.SaveAsync(cal);
            HttpContext.Response.StatusCode = 201;
            return Json(cal);
        }
        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestCalender()
        {
            Calender calender = await _ddbService.LatestCalender();
            HttpContext.Response.StatusCode = 201;
            return Json(calender);
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
                MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl("mongodb://smartmeet:password123@ds034807.mlab.com:34807/smartmeeting"));
                if (true)
                {
                    settings.SslSettings = new SslSettings { EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12 };
                }
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
            IEnumerable<Calender> calenderQuery = CalenderCollection.Find(d => d.Id == id).ToEnumerable();
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
