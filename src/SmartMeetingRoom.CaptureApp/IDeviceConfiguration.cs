using System;

namespace SmartMeetingRoom.CaptureApp
{
    internal interface IDeviceConfiguration
    {
        Guid DeviceId { get; }
        string DeviceIdString { get; }
        string DeviceKey { get; }
    }
}