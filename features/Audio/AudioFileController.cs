using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading.Tasks;
using Features.Audio.Entities;
using System.Collections.Generic;
using Features.Audio.Extension;
using Features.Audio.Exceptions;
using Data;

namespace Features.Audio.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AudioController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAudioServices _audioServices;

        public AudioController(AppDbContext context, IAudioServices audioServices)
        {
            _context = context;
            _audioServices = audioServices;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadAudio(IFormFile file, [FromForm] string name)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("No file -_-.");

                if (string.IsNullOrEmpty(name))
                    return BadRequest("Why no name?.");

                var (isValid, errorMessage) = await _audioServices.ValidateAudioFile(file);
                if (!isValid)
                {
                    return BadRequest(new { message = errorMessage });
                }

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
            catch (AudioValidationException ex)
            {
                return BadRequest(new { message = ex.Message, details = ex.ValidationDetails });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An unexpected error occurred while processing the audio file." });
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

        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadAudio(int id)
        {
            try
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
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while downloading the audio file." });
            }
        }

        [HttpGet("{id}/size")]
        public async Task<IActionResult> GetAudioFileSizeKB(int id)
        {
            try
            {
                var audFile = await _context.AudioFiles.FindAsync(id);
                if (audFile == null)
                {
                    return NotFound(new { message = "No such audio file exists (at least not with that id)" });
                }

                var fileSizeKB = audFile.GetAudioFileSize();
                return Ok(new { fileSize = $"{fileSizeKB:F2} KB" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while calculating the file size." });
            }
        }
    }
}
