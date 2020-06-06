namespace VideoClipEditor
{
    internal class VideoEditorSettings
    {
        public static readonly VideoQualityCommand[] videoQualityCommands;
        internal static VideoQualityCommand currentQuality;

        static VideoEditorSettings()
        {
            videoQualityCommands = new VideoQualityCommand[3];

            // Low Quality / Old Version
            var cmd = new VideoQualityCommand();
            cmd.commandLabel = "720low";
            cmd.command = (start, currentFile, length, isMuted, FileName) => "-ss " + start + " -i \""
                    + currentFile + "\" -t "
                    + length
                    + " -c:v libvpx-vp9 -crf 4 -b:v 1500K -vf scale=1280:-1 "
                    + (isMuted ? "-an " : "")
                    + "\"" + FileName + "\"";
            videoQualityCommands[0] = cmd;

            // Higher Bitrate 720p
            cmd = new VideoQualityCommand();
            cmd.commandLabel = "720high";
            cmd.command = (start, currentFile, length, isMuted, FileName) => "-ss " + start + " -i \""
                    + currentFile + "\" -t "
                    + length
                    + " -c:v libvpx-vp9 -crf 4 -b:v 3000K -vf scale=1280:-1 "
                    + (isMuted ? "-an " : "")
                    + "\"" + FileName + "\"";
            videoQualityCommands[1] = cmd;

            // Higher bitrate and 1080 resolution
            cmd = new VideoQualityCommand();
            cmd.commandLabel = "1080high";
            cmd.command = (start, currentFile, length, isMuted, FileName) => "-ss " + start + " -i \""
                    + currentFile + "\" -t "
                    + length
                    + " -c:v libvpx-vp9 -crf 4 -b:v 6000K -vf scale=1920:-1 "
                    + (isMuted ? "-an " : "")
                    + "\"" + FileName + "\"";
            videoQualityCommands[2] = cmd;

            currentQuality = videoQualityCommands[0];
        }
    }

    internal class VideoQualityCommand
    {
        public string commandLabel;
        internal System.Func<int, string, int, bool, string, string> command;
    }
}