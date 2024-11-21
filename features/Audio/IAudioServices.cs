public interface IAudioServices
{
    Task<(bool isValid, string errorMessage)> ValidateAudioFile(IFormFile file);
}
