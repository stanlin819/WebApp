using WebApp.Models.UserModel;
namespace WebApp.Models.UploadedFileModel;
using System.Text.Json.Serialization;


public class UploadedFile : IObject
{
    public int Id { get; set; }
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public long FileSize { get; set; }
    public int UserId { get; set; }
    public bool isVideo { get; set; }
    public DateTime UploadedAt { get; set; }
    [JsonIgnore]
    public User User { get; set; }
}