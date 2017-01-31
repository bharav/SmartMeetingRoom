using Newtonsoft.Json;
using System.Collections.Generic;

namespace SmartMeetingRoom.Common.DTO
{
    public class Device
    {

        public string id{get; set;}

        public string DeviceId { get; set; }

        public string MeetingRoomId { get; set; }

        public string CameraId { get; set; }

        public double Confidence { get; set; }
       
    }
}
