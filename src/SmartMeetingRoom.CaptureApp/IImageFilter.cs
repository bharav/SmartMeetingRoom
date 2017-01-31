using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using System.IO;

namespace SmartMeetingRoom.CaptureApp
{
    internal interface IImageFilter
    {
        Task ProcessImageAsync(BitmapDecoder bitmapDecoder, IRandomAccessStream imageStream, string cameraId);
        Task ProcessImageAsync(Stream imageStream, string cameraId, string empName, string empDeptName);

    }
}