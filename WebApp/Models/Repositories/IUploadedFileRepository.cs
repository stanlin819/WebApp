using WebApp.Models.UploadedFileModel;

namespace WebApp.Models.Repositories;

public interface IUploadedFileRepository : IRepository<UploadedFile>
{
    Task Add(IEnumerable<UploadedFile> uploadedFiles);
    Task<IEnumerable<UploadedFile>> GetUserFile(int userId);
    Task Delete(IEnumerable<UploadedFile> uploadedFiles);
}