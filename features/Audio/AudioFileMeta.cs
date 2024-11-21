using Features.Audio.Entities;

namespace Features.Audio.Extension {
    public static class AudioFileExtensions {

        // kilobytes
        public static double GetAudioFileSize(this AudioFileEntity audioFile) {
            return (double)audioFile.FileData.Length / 1024;
        }
    }
}
