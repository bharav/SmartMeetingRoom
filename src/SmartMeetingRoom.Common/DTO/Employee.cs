using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SmartMeetingRoom.Common.DTO
{
    public class Employee
    {
        [JsonProperty(PropertyName="id")]
        public string Id { get; set; }

        public string EmpName { get; set; }

        public string PersonId { get; set; }

        public string BlobName { get; set; }

        public string Department { get; set; }

    }
}
