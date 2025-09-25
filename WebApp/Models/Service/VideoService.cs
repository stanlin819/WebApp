using WebApp.Models.Options;
using Microsoft.Extensions.Options;
using WebApp.Models.PlayBackLogModel;
using WebApp.Models.Repositories;
using WebApp.Models.UploadedFileModel;

namespace WebApp.Models.Service;

public class VideoService : IVideoService
{
    private readonly IPlayBackLogRepository _repo;
    private readonly IUploadedFileRepository _file_repo;
    private readonly string _uploadPath;

    public VideoService(IPlayBackLogRepository repo, IUploadedFileRepository file_repo, IOptions<VideoSettingOptions> options)
    {
        _repo = repo;
        _file_repo = file_repo;
        _uploadPath = options.Value.UploadPath;

        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
        }

    }

    public async Task<string> UploadChunk(IFormFile chunk, string fileId, int chunkIndex)
    {
        var tmpDir = Path.Combine(_uploadPath, "tmp_chunks");
        if (!Directory.Exists(tmpDir)) Directory.CreateDirectory(tmpDir);

        var filePath = Path.Combine(tmpDir, $"{fileId}.part{chunkIndex}");

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await chunk.CopyToAsync(stream);
        }

        return filePath;
    }

    public async Task<string> Merge(string fileId, string filename, int totalChunks, int userId)
    {
        var tmpDir = Path.Combine(_uploadPath, "tmp_chunks");
        var path = Path.Combine(_uploadPath, $"user_{userId}");

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        var filePath = Path.Combine(path, Path.GetFileName(filename));

        int count = 1;
        string fileNameWithoutExt = Path.GetFileNameWithoutExtension(filename);
        string extension = Path.GetExtension(filename);
        while (System.IO.File.Exists(filePath))
        {
            filename = $"{fileNameWithoutExt}({count++}){extension}";
            filePath = Path.Combine(path, filename);
        }

        try
        {
            using (var outputStream = new FileStream(filePath, FileMode.Create))
            {
                for (int i = 0; i < totalChunks; i++)
                {
                    string partPath = Path.Combine(tmpDir, $"{fileId}.part{i}");
                    using (var fs = new FileStream(partPath, FileMode.Open))
                    {
                        await fs.CopyToAsync(outputStream);
                    }
                }
            }

            for (int i = 0; i < totalChunks; i++)
            {
                string partPath = Path.Combine(tmpDir, $"{fileId}.part{i}");
                System.IO.File.Delete(partPath);
            }

            var uf = new UploadedFile
            {
                FileName = Path.GetFileName(filename),
                FilePath = filePath,
                FileSize = new FileInfo(filePath).Length,
                UserId = userId,
                isVideo = true,
                UploadedAt = DateTime.Now
            };

            await _file_repo.Add(uf);

            // System.IO.File.Delete(filePath);
            return filePath;
        }
        catch (SystemException ex)
        {
            return ex.Message;
        }
    }

    public async Task<bool> CheckVideo(string filename, int userId)
    {
        var path = Path.Combine(_uploadPath, $"user_{userId}");
        var filePath = Path.Combine(path, Path.GetFileName(filename));
        if (File.Exists(filePath))
        {
            var uf = new UploadedFile
            {
                FileName = Path.GetFileName(filename),
                FilePath = filePath,
                FileSize = new FileInfo(filePath).Length,
                UserId = userId,
                isVideo = true,
                UploadedAt = DateTime.Now
            };

            await _file_repo.Add(uf);
            return true;
        }
        return false;
    }
    public async Task Record(PlayBackLog log)
    {
        var l = await _repo.Getlog(log.UserId, log.VideoId);
        if (l == null)
        {
            await _repo.Add(log);
        }
        else
        {
            l.LastPosition = log.LastPosition;
            l.UpdateAt = log.UpdateAt;
            await _repo.Update(l);
        }
    }

    public async Task<PlayBackLog> GetLog(int userId, string videoId)
    {
        return await _repo.Getlog(userId, videoId);
    }
    public async Task<List<UploadedFile>> GetUserVideos(int userId)
    {
        var fileList = await _file_repo.GetUserFile(userId);
        var videoList = fileList.Where(fl => fl.isVideo).ToList();
        if (videoList == null)
        {
            return new List<UploadedFile>();
        }

        return videoList;
    }

    public async Task DeleteUserVideo(int userId)
    {
        var path = _uploadPath + $"/user_{userId}";

        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }
}