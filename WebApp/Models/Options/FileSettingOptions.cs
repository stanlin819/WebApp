namespace WebApp.Models.Options;

public class FileSettingOptions
{
    public string UploadPath { get; set; }
    public long MaxSizeMB { get; set; }
    public List<string> AllowedExtensions { get; set; }
    
}