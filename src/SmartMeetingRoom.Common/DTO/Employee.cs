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
    public class Employee
    {
        public ObjectId Id { get; set; }
        [BsonElement("EmpName")]
        public string EmpName { get; set; }
        [BsonElement("PersonId")]
        public string PersonId { get; set; }
        [BsonElement("BlobName")]
        public string BlobName { get; set; }
        [BsonElement("Department")]
        public string Department { get; set; }

    }
}
