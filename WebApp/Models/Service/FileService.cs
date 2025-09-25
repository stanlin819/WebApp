namespace WebApp.Models.Service;
using Microsoft.Extensions.Options;
using WebApp.Models.Options;
using System.IO.Compression;
using WebApp.Models.UploadedFileModel;
using WebApp.Models.Repositories;
using WebApp.Models.PlayBackLogModel;
using System.Threading.Tasks;

public class FileService : IFileService
{
    private readonly IUploadedFileRepository _repo;
    private readonly IPlayBackLogRepository _log_repo;
    private readonly string _uploadPath;
    private readonly string _videoPath;
    private readonly long _maxSize;

    private readonly List<string> _allowedExtensions;

    public FileService(IUploadedFileRepository repo, IPlayBackLogRepository log_repo, IOptions<FileSettingOptions> options)
    {
        _repo = repo;

        _log_repo = log_repo;

        _uploadPath = options.Value.UploadPath;

        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
        }

        _maxSize = options.Value.MaxSizeMB * 1024 * 1024;
        _allowedExtensions = options.Value.AllowedExtensions;
    }

    public async Task<String> UploadFile(List<IFormFile> files, int userId)
    {
        if (files == null || files.Count == 0)
        {
            return "Please select at least one file";
        }

        var path = Path.Combine(_uploadPath, $"user_{userId}");
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        var ufList = new List<UploadedFile>();
        var now = DateTime.Now;

        foreach (var file in files)
        {
            // 0KB 檔案檢查
            if (file.Length == 0)
            {
                return $"The file {file.FileName} is empty and cannot be uploaded.";
            }

            // 副檔名檢查
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!_allowedExtensions.Contains(ext))
            {
                return $"The file {file.FileName} type is not allowed to upload";
            }



            // 防止路徑穿越攻擊
            var safeFileName = Path.GetFileName(file.FileName);


            if (ext.Equals(".zip"))
            {
                using (var stream = file.OpenReadStream())
                using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Read))
                {
                    foreach (var entry in archive.Entries)
                    {
                        var sfn = Path.GetFileName(entry.FullName);
                        string destinationPath = Path.Combine(path, sfn);

                        int count = 1;
                        string fileNameWithoutExt = Path.GetFileNameWithoutExtension(sfn);
                        string extension = Path.GetExtension(sfn);
                        while (System.IO.File.Exists(destinationPath))
                        {
                            sfn = $"{fileNameWithoutExt}({count++}){extension}";
                            destinationPath = Path.Combine(path, sfn);
                        }

                        if (!string.IsNullOrEmpty(entry.Name))
                        {
                            entry.ExtractToFile(destinationPath, overwrite: true);
                            var uf = new UploadedFile
                            {
                                FileName = sfn,
                                FilePath = destinationPath,
                                FileSize = entry.Length,
                                UserId = userId,
                                UploadedAt = now
                            };
                            ufList.Add(uf);
                        }
                    }
                }
                continue;
            }

            var filePath = Path.Combine(path, safeFileName);
            if (file.Length > _maxSize)
            {
                // 壓縮檔案
                var zipFileName = Path.GetFileNameWithoutExtension(safeFileName) + ".zip";
                var zipFilePath = Path.Combine(path, zipFileName);

                int count = 1;
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(safeFileName);
                while (System.IO.File.Exists(zipFilePath))
                {
                    zipFileName = $"{fileNameWithoutExt}({count++}).zip";
                    zipFilePath = Path.Combine(path, zipFileName);
                }

                using (var zipStream = new FileStream(zipFilePath, FileMode.Create))
                using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
                {
                    var entry = archive.CreateEntry(safeFileName, CompressionLevel.Optimal);
                    using (var entryStream = entry.Open())
                    {
                        await file.CopyToAsync(entryStream);
                    }
                }

                var uf = new UploadedFile
                {
                    FileName = zipFileName,
                    FilePath = zipFilePath,
                    FileSize = file.Length,
                    UserId = userId,
                    UploadedAt = now
                };
                ufList.Add(uf);
            }
            else
            {
                int count = 1;
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(safeFileName);
                string extension = Path.GetExtension(safeFileName);
                while (System.IO.File.Exists(filePath))
                {
                    safeFileName = $"{fileNameWithoutExt}({count++}){extension}";
                    filePath = Path.Combine(path, safeFileName);
                }
                // 直接存放
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var uf = new UploadedFile
                {
                    FileName = safeFileName,
                    FilePath = filePath,
                    FileSize = file.Length,
                    UserId = userId,
                    UploadedAt = now
                };
                ufList.Add(uf);
            }
        }

        await _repo.Add(ufList);
        return "successfully";
    }

    public byte[] DownloadFile(string fileName, int userId)
    {
        var path = _uploadPath + $"/user_{userId}";
        var filePath = Path.Combine(path, fileName);

        var fileBytes = System.IO.File.ReadAllBytes(filePath);
        return fileBytes;
    }

    public MemoryStream DownloadMultipleFiles(string[] selectedFiles, int userId)
    {
        var path = _uploadPath + $"/user_{userId}";

        using (var memoryStream = new MemoryStream())
        {
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                foreach (var fileName in selectedFiles)
                {
                    var filePath = Path.Combine(path, fileName);
                    if (System.IO.File.Exists(filePath))
                    {
                        archive.CreateEntryFromFile(filePath, fileName);
                    }
                }
            }

            return memoryStream;
        }
    }

    public async Task<string> RemoveFiles(IEnumerable<string> fileName, int userId)
    {
        var userFiles = await _repo.GetUserFile(userId);
        var deleteFiles = userFiles.Where(uf => fileName.Contains(uf.FileName));
        var deleteLog = new List<PlayBackLog>();

        try
        {
            foreach (var f in deleteFiles)
            {
                if (f.isVideo)
                {
                    var log = await _log_repo.Getlog(f.UserId, f.FileName);
                    if (log != null)
                        deleteLog.Add(log);
                }
                var path = f.FilePath;
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
            }
        }
        catch (Exception e)
        {
            return e.Message;
        }
        if (deleteLog.Any())
        {
            await _log_repo.Delete(deleteLog);
        }
        await _repo.Delete(deleteFiles);
        return "successfully";
    }

    public async Task RemoveUserFold(int userId)
    {
        var userFile = await _repo.GetUserFile(userId);
        if (userFile != null && userFile.Any())
        {
            var deleteFile = new List<string>();
            foreach (var f in userFile)
            {
                deleteFile.Add(f.FileName);
            }
            await RemoveFiles(deleteFile, userId);
        }
        var path = _uploadPath + $"/user_{userId}";

        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }

    }

    public async Task RemoveAllFile()
    {
        var files =  await _repo.GetAll();
        var groupedFiles = files.GroupBy(f => f.UserId).ToList();
        foreach (var group in groupedFiles)
        {
            var deleteFile = new List<string>();
            foreach (var f in group)
            {
                deleteFile.Add(f.FileName);
            }
            await RemoveFiles(deleteFile, group.Key);
        }
        Directory.Delete(_uploadPath);
        Directory.CreateDirectory(_uploadPath);
    }

    public async Task<List<UploadedFile>> GetUserFiles(int userId)
    {
        var fileList = await _repo.GetUserFile(userId);
        fileList = fileList.Where(f => !f.isVideo);
        if (fileList == null)
        {
            return new List<UploadedFile>();
        }

        return fileList.ToList();
    }

    public async Task<UploadedFile> Get(int id)
    {
        return await _repo.Get(id);
    }
}