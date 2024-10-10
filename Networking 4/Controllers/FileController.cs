using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Networking_4.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly ILogger<FileController> _logger;

        public FileController(ILogger<FileController> logger)
        {
            _logger = logger;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload()
        {
            if (!Request.HasFormContentType)
            {
                return BadRequest("Request does not contain a valid form.");
            }

            var file = Request.Form.Files["file"];
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            try
            {
                var currentDirectory = Directory.GetCurrentDirectory();
                var assetsDirectory = Path.Combine(currentDirectory, "Assets");

                if (!Directory.Exists(assetsDirectory))
                {
                    Directory.CreateDirectory(assetsDirectory);
                }

                // Формируем полный путь для сохранения файла
                var filePath = Path.Combine(assetsDirectory, file.FileName);

                // Используем FileStream для записи
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return Ok(new { filePath });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error uploading file.");
            }
        }


        [HttpGet("download/{fileName}")]
        public IActionResult Download(string fileName)
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var assetsDirectory = Path.Combine(currentDirectory, "Assets");

            var filePath = Path.Combine(assetsDirectory, fileName);

            // Проверяем, существует ли файл
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("File not found.");
            }

            // Читаем файл в массив байтов
            var data = System.IO.File.ReadAllBytes(filePath);
            var contentType = "application/octet-stream";
            var fileDownloadName = fileName;

            // Возвращаем файл с указанием типа содержимого и имени
            return File(data, contentType, fileDownloadName);
        }

    }
}
