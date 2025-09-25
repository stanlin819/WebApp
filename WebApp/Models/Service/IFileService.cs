using WebApp.Models.UploadedFileModel;
namespace WebApp.Models.Service;

public interface IFileService
{
    Task<String> UploadFile(List<IFormFile> files, int userId);
    byte[] DownloadFile(string fileName, int userId);
    MemoryStream DownloadMultipleFiles(string[] selectedFiles, int userId);
    Task<string> RemoveFiles(IEnumerable<string> fileName, int userId);
    Task RemoveUserFold(int userId);
    Task RemoveAllFile();
    Task<List<UploadedFile>> GetUserFiles(int userId);
    Task<UploadedFile> Get(int id);

}