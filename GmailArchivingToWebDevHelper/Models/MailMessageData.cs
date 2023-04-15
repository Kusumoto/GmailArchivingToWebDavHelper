﻿namespace GMailArchivingToWebDavHelper.Models
{
    public class MailMessageData
    {
        public string Header { get; set; } = "";
        public string Body { get; set; } = "";
        public DateTimeOffset Date { get; set; }
        public List<AttachmentData> Attachments { get; set; } = new List<AttachmentData>();
        public int Index { get; set; }
    }
}
