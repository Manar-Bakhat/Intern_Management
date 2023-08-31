using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Intern_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {

        private readonly string UploadsPath = "C:\\Users\\hp\\source\\repos\\Intern_Management\\Intern_Management\\uploads";

        [HttpGet("Download/{fileName}")]
        public IActionResult Download(string fileName)
        {
            string filePath = Path.Combine(UploadsPath, fileName);

            if (System.IO.File.Exists(filePath))
            {
                var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                return File(fileStream, "application/octet-stream", fileName);
            }
            else
            {
                return NotFound();
            }
        }
    }
}
