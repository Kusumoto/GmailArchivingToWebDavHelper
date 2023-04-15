namespace GMailArchivingToWebDavHelper.Core.Interface
{
    public interface IFileManager
    {
        Task UploadFileToWebDav(string filename, string path, MemoryStream stream);
    }
}
