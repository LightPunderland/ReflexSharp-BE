using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading.Tasks;
using Features.Audio.Entities;
using System.Collections.Generic;
using Features.Audio.Extension;
using Data;

namespace Features.Audio.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AudioController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AudioController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadAudio(IFormFile file, [FromForm] string name)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            if (string.IsNullOrEmpty(name))
                return BadRequest("Audio name is required.");

            using (var readStream = file.OpenReadStream())
            {
                using (var memoryStream = new MemoryStream())
                {
                    await readStream.CopyToAsync(memoryStream);

                    var audioFile = new AudioFileEntity
                    {
                        Name = name,
                        FileData = memoryStream.ToArray()
                    };

                    _context.AudioFiles.Add(audioFile);
                    await _context.SaveChangesAsync();

                    return Ok(new { message = "Audio uploaded successfully!", audioId = audioFile.Id });
                }
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAudio(int id)
        {
            var audioFile = await _context.AudioFiles.FindAsync(id);
            if (audioFile == null)
                return NotFound(new { message = "Audio file not found." });

            var memoryStream = new MemoryStream(audioFile.FileData);
            return new FileStreamResult(memoryStream, "audio/mpeg")
            {
                FileDownloadName = $"{audioFile.Name}.mp3"
            };
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllAudioFiles()
        {
            var audioFiles = await _context.AudioFiles.ToListAsync();
            var audioList = new List<object>();

            foreach (var audioFile in audioFiles)
            {
                audioList.Add(new { audioFile.Id, audioFile.Name });
            }

            return Ok(audioList);
        }

        // in the credits we will include estudiocafofo@globo.com  for producing the menu music in the version
        // of the game (downloaded from https://opengameart.org/content/happy-game-theme-demo)
        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadAudio(int id)
        {
            var audioFile = await _context.AudioFiles.FindAsync(id);
            if (audioFile == null)
                return NotFound(new { message = "Audio file not found." });

            var filePath = Path.Combine("Assets", $"{audioFile.Name}.mp3");
            using (var writeStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                await writeStream.WriteAsync(audioFile.FileData, 0, audioFile.FileData.Length);
            }

            var readStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return new FileStreamResult(readStream, "audio/mpeg")
            {
                FileDownloadName = $"{audioFile.Name}.mp3"
            };
        }

        [HttpGet("{id}/size")]
        public async Task<IActionResult> GetAudioFileSizeKB(int id) {
            var audFile = await _context.AudioFiles.FindAsync(id);
            if (audFile == null) {
                return NotFound(new {message = "No such audio file exists(atleast not with that id"});
            }

            var fileSizeKB = audFile.GetAudioFileSize(); //extension metod:P

            return Ok(new {fileSize = $"{fileSizeKB:F2} KB" });
        }
    }
}
