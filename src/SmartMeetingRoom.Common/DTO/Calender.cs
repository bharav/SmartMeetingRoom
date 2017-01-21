﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SmartMeetingRoom.Common.DTO
{
    public class Calender
    {

        [JsonProperty(PropertyName = "id")]
        public string Id
        {
            get;
            set;
        }


        public string MeetingRoomId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public MeetingInvitee[] Invitees { get; set; }

        public MeetingAttendee[] MeetingAttendees { get; set; }

        public double Confidence { get; set; }

    }

    public class MeetingInvitee {
        public string PersonId { get; set; }
    }

    public class MeetingAttendee{
            public string PersonId { get; set; }
            public DateTime TimeEntered { get; set; }
            public string Smile { get; set; }
            public string Gender { get; set; }
            public string IsInvitee { get; set; }

    }
}
