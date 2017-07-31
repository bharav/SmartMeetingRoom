using Newtonsoft.Json;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace SmartMeetingRoom.Common.DTO
{
    public class Device
    {
        
        public ObjectId id{get; set;}
        [BsonElement("DeviceId")]
        public string DeviceId { get; set; }
        [BsonElement("MeetingRoomId")]
        public string MeetingRoomId { get; set; }
        [BsonElement("CameraId")]
        public string CameraId { get; set; }
        [BsonElement("Confidence")]
        public double Confidence { get; set; }
       
    }
}
