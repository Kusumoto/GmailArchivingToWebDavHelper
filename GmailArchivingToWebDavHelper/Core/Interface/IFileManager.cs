using GMailArchivingToWebDavHelper.Models;

namespace GMailArchivingToWebDavHelper.Core.Interface;

public interface IFileManager
{
    Task UploadFileToWebDav(string filename, FilterSettingData filterSetting, MemoryStream stream);
}