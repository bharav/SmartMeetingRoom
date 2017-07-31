using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SmartMeetingRoom.Common.DTO
{
    public class Calender
    {
        private DateTime created = DateTime.Now;
        public ObjectId Id {get; set;}
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
}
