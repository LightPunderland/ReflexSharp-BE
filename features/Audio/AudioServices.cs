using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Features.Audio.Exceptions;
using Microsoft.Extensions.Logging;

    public class AudioServices : IAudioServices
    {
        private const int MaxFileSize = 16; // In MB

        // Added for showcase, dont know why we would need wav
        private readonly string[] _validAudioExtensions = { ".mp3", ".wav" };
        private readonly ILogger<AudioServices> _logger;

        public AudioServices(ILogger<AudioServices> logger)
        {
            _logger = logger;
        }

        public async Task<(bool isValid, string errorMessage)> ValidateAudioFile(IFormFile file)
        {
            try
            {
                // Check file size
                var fileSizeInMB = file.Length / (1024.0 * 1024.0);
                if (fileSizeInMB > MaxFileSize)
                {
                    throw new AudioFileSizeException(file.FileName, fileSizeInMB, MaxFileSize);
                }

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!_validAudioExtensions.Contains(extension))
                {
                    throw new AudioValidationException(
                        "Invalid file type",
                        file.FileName,
                        $"Only {string.Join(", ", _validAudioExtensions)} files are allowed."
                    );
                }

                // async implimentation here!
                // Waits for file content validation to finish before uplaoding it.
                using (var stream = file.OpenReadStream())
                {
                    var buffer = new byte[4096];
                    var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                    if (bytesRead == 0)
                    {
                        throw new AudioValidationException(
                            "Empty file",
                            file.FileName,
                            "The audio file appears to be empty."
                        );
                    }


                }

                return (true, string.Empty);
            }
            catch (AudioValidationException ex)
            {
                // Log the validation exception
                _logger.LogError(
                    "Audio validation failed for {FileName}: {Message}. Details: {Details}",
                    ex.AudioFileName,
                    ex.Message,
                    ex.ValidationDetails
                );
                return (false, ex.Message);
            }
            catch (Exception ex)
            {
                // Log unexpected exceptions
                _logger.LogError(ex, "Unexpected error during audio validation for {FileName}", file?.FileName);
                return (false, "An unexpected error occurred during file validation.");
            }
        }
    }
