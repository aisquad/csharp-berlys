using ImapX;
using ImapX.Collections;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace capp1
{
    struct AttachementStruct
    {
        public string newFilename {get; set; }
        public Attachment attachment { get; set; }

    }

    class GetMail
    {
        private readonly string server;
        private readonly string email;
        private readonly string password;
        private readonly string attachmentsDirPath;
        private readonly Config config;
        private readonly string lastData;

        private ImapClient client;

        public GetMail()
        {
            ConfigRetriever configRetriever = new ConfigRetriever();
            config = configRetriever.Load();
            server = config.server;
            email = config.email;
            password = config.password;
            attachmentsDirPath = $@"{configRetriever.userPath}\OneDrive\Scripts\data\berlys\attachments\";
        }

        private bool ConnectAndLogIn()
        {
            client = new ImapClient(server, 993, true);
            return client.Connect() && client.Login(email, password);
        }

        private MessageCollection FromFolder(string folder)
        {
             return client.Folders[folder].Messages;
        }

        private void DownloadAttachements(MessageCollection messages, DateTime? since = null, DateTime? before = null)
        {
            string criteria = "";
            if (since != null)
                criteria += $"SENTSINCE {since?.ToString("dd-MMM-yyy", CultureInfo.InvariantCulture)}";

            if (before != null)
                criteria += $" SENTBEFORE {before?.ToString("dd-MMM-yyy", CultureInfo.InvariantCulture)}";

            if (criteria != "")
                messages.Download(criteria);
            else
                messages.Download();

            Queue<AttachementStruct> attachementQueue = new Queue<AttachementStruct>();
            bool sheetHasBeenInserted = false;
            AttachementStruct attExt = new AttachementStruct();
            string basename = Path.GetFileNameWithoutExtension(config.loadingFilename);

            foreach (Message message in messages)
            {
                Console.WriteLine($"MESSAGE FROM: {message.From.Address.ToString()} {message.Date:yyyy-MM-dd}");
                Console.WriteLine($"ATT LENGTH: {message.Attachments.Length} RSC LENGTH: {message.EmbeddedResources.Length}");

                var attachements = (message.Attachments.Length > 0) ? message.Attachments : message.EmbeddedResources;
                foreach (var attachment in attachements)
                {
                    attachment.Download();
                    attExt.attachment = attachment;
                    if (attachment.FileName == config.loadingFilename)
                    {
                        DateTime? date = message.Date;
                        date = date?.AddDays(1);
                        string filedate = date?.ToString("yyyy-MM-dd");
                        attExt.newFilename = $"{basename} {filedate}.txt";
                        attachementQueue.Enqueue(attExt);
                    }
                    else if (attachment.FileName == config.sheetFilename && !sheetHasBeenInserted)
                    {
                        attExt.newFilename = attachment.FileName.Replace("ç", "");
                        attachementQueue.Enqueue(attExt);
                        sheetHasBeenInserted = true;
                    }

                }
                Console.WriteLine();
            }
            foreach (AttachementStruct att in attachementQueue)
                att.attachment.Save(attachmentsDirPath, att.newFilename);
        }

        private void Logout()
        {
            client.Logout();
            client.Disconnect();
            Console.WriteLine("Disconnected.");
        }
        public void Run()
        {
            if (ConnectAndLogIn())
            {
                Console.WriteLine("Logged in!");
                DownloadAttachements(FromFolder(config.folder), new DateTime (2020, 02, 07));
                Logout();
            }
        }
    }
}
