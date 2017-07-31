using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SmartMeetingRoom.Common.DTO
{
    public class DeviceCamera
    {
        public Object Id { get; set; }

        /// <summary>
        ///     If confidence is lower than ConfidenceLimit, new person is created
        /// </summary>
        /// 
        [BsonElement("ConfidenceLimit")]
        public double ConfidenceLimit { get; set; }

        public DeviceCamera()
        {
        }
        public DeviceCamera(string id)
        {
            Id = id;
        }

    }
}
