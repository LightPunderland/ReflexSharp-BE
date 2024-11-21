using System;

namespace Features.Audio.Exceptions
{
    public class AudioValidationException : Exception
    {
        public string AudioFileName { get; }
        public string ValidationDetails { get; }

        public AudioValidationException(string message, string audioFileName, string validationDetails, Exception innerException = null)
            : base(message, innerException)
        {
            AudioFileName = audioFileName;
            ValidationDetails = validationDetails;
        }
    }

    public class AudioFileSizeException : AudioValidationException
    {
        public double FileSize { get; }
        public double MaxAllowedSize { get; }

        public AudioFileSizeException(string audioFileName, double fileSize, double maxAllowedSize)
            : base(
                $"File size {fileSize:F2}MB exceeds maximum limit of {maxAllowedSize}MB",
                audioFileName,
                $"Current size: {fileSize:F2}MB, Max allowed: {maxAllowedSize}MB")
        {
            FileSize = fileSize;
            MaxAllowedSize = maxAllowedSize;
        }
    }
}
