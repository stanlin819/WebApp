using WebApp.Models.PlayBackLogModel;
using WebApp.Models.UploadedFileModel;

namespace WebApp.Models.Service;

public interface IVideoService
{
    Task<string> UploadChunk(IFormFile chunk, string fileId, int chunkIndex);
    Task<string> Merge(string fileId, string filename, int totalChunks, int userId);
    Task<bool> CheckVideo(string filename, int userId);
    Task Record(PlayBackLog log);
    Task<PlayBackLog> GetLog(int userId, string videoId);
    Task<List<UploadedFile>> GetUserVideos(int userId);
    Task DeleteUserVideo(int userId);
}