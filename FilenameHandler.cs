using System;
using System.IO;

namespace capp1
{
    class FilenameHandler
    {
        private string _path;
        private Config config;

        public FilenameHandler()
        {
            ConfigRetriever configRetriever = new ConfigRetriever();
            config = configRetriever.Load();
            string userPath = configRetriever.userPath;
            TemporaryDirPath = $@"{userPath}\AppData\Local\Temp\";
            DownloadsDirPath = $@"{userPath}\Downloads\";
            DataDirPath = $@"{userPath}\OneDrive\Scripts\data\berlys\";
            AttachmentsDirPath = $@"{userPath}\OneDrive\Scripts\berlys\data\attachments\";
        }

        public string TemporaryDirPath { get; private set; }

        public string DownloadsDirPath { get; private set; }

        public string DataDirPath { get; private set; }

        public string AttachmentsDirPath { get; private set; }

        public string Filename { get; set; }

        public string AbsoluteFilename { get; private set; }

        public void AppendToDataDirPath(string path)
        {
            DataDirPath += path;
            _path = DataDirPath;
            AbsoluteFilename = _path + Filename;
        }

        public string FromDownloadsDir()
        {
            _path = DownloadsDirPath;
            Filename = config.loadingFilename;
            AbsoluteFilename = _path + Filename;
            return AbsoluteFilename;
        }

        public string FromDataDir()
        {
            string[] files = Directory.GetFiles(DataDirPath, "*.txt", SearchOption.AllDirectories);
            AbsoluteFilename = files[^1];
            _path = Path.GetDirectoryName(AbsoluteFilename);
            Filename = Path.GetFileName(AbsoluteFilename);
            return AbsoluteFilename;
        }
        public string ToDataDir()
        {
            _path = DataDirPath;
            AbsoluteFilename = _path + Filename;
            return AbsoluteFilename;
        }

        public string ToAttachmentsDir()
        {
            _path = AttachmentsDirPath;
            AbsoluteFilename = _path + Filename;
            return AbsoluteFilename;
        }
    }
}
