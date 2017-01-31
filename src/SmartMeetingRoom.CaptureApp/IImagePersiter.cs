using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SmartMeetingRoom.CaptureApp
{
    internal interface IImagePersiter
    {
        Task PersistAsync(IEnumerable<Stream> streams, string cameraId);
        Task PersistAsync(Stream fileImage, string cameraId, string empName, string empDeptName);
    }
}