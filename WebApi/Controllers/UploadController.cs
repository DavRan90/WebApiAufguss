using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public UploadController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpGet("{fileName}")]
        public IActionResult GetImage(string fileName)
        {
            var path = Path.Combine("./wwwroot/", "upload/", fileName);

            if (!System.IO.File.Exists(path))
                return NotFound("File not found");

            var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            var contentType = "image/jpeg"; // or use MimeTypes.GetMimeType(fileName)

            return File(fileStream, contentType);
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            // Save to wwwroot/uploads (or wherever you like)
            var uploadsPath = Path.Combine("./wwwroot/", "upload");

            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);

            var filePath = Path.Combine(uploadsPath, file.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(new { message = "File uploaded successfully.", path = $"/upload/{file.FileName}" });
        }
    }
}
