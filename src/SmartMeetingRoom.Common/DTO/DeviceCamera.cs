using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartMeetingRoom.Common.DTO
{
    public class DeviceCamera
    {
        public string Id { get; set; }

        /// <summary>
        ///     If confidence is lower than ConfidenceLimit, new person is created
        /// </summary>
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
