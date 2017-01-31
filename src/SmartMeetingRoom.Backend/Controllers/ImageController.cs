using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using SmartMeetingRoom.Common;
using SmartMeetingRoom.Common.Services;
using SmartMeetingRoom.Common.DTO;
using Microsoft.Extensions.Configuration;
// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace SmartMeetingRoom.Backend.Controllers
{
    [Route("api/[controller]")]
    public class ImageController : Controller
    {
        string storageConnectionString;
        string containerName = "faces";
        private readonly AppConfiguration _appConfigService;

        public ImageController(IAppConfiguration config) {
            storageConnectionString = config.StorageConnectionString;
        }
        // GET: api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public IActionResult GetImage(string id)
        {
            var result = DownloadBlockBlob(id);
            return File(ReadFully(result), "image/jpeg");
        }

        public MemoryStream DownloadBlockBlob(string blobName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer _container = blobClient.GetContainerReference(containerName);
            CloudBlockBlob blockBlob = _container.GetBlockBlobReference(blobName);
            MemoryStream resultStream = new MemoryStream();
            blockBlob.DownloadToStream(resultStream);
            resultStream.Seek(0, SeekOrigin.Begin);
            return resultStream;
        }
        public byte[] ReadFully(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}
