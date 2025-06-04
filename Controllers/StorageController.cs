using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Mvc;

namespace sendbol_videoshop.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StorageController : ControllerBase
    {
        private readonly ILogger<StorageController> _logger;

        public StorageController(ILogger<StorageController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetFile(string filename)
        {
            var client = StorageClient.Create();
            var stream = new MemoryStream();
            var obj = await client.DownloadObjectAsync("videoshop_image_storage_bucket_1", filename, stream);
            stream.Position = 0; // Reset the stream position to the beginning

            return File(stream, obj.ContentType, obj.Name);
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var client = StorageClient.Create();
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0; // Reset the stream position to the beginning

            var obj = await client.UploadObjectAsync(
                "videoshop_image_storage_bucket_1",
                file.FileName,
                file.ContentType,
                stream);
                
            return Ok(new { obj.Name, obj.MediaLink });
        }
    }
}