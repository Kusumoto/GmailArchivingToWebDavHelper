namespace GMailArchivingToWebDavHelper.Models;

public class AttachmentData
{
    public string Filename { get; set; } = "";
    public MemoryStream DataStream { get; set; } = new();
}