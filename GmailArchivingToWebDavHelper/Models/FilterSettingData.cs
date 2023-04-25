namespace GMailArchivingToWebDavHelper.Models;

public class FilterSettingData
{
    public string HeaderRegEx { get; set; } = "";
    public string FilePath { get; set; } = "";
    public string EmailFrom { get; set; } = "";
    public string BodyRegEx { get; set; } = "";
    public string FileFormatFilter { get; set; } = "";
}