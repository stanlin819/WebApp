using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models.Service;
using WebApp.Models.PlayBackLogModel;
using WebApp.Models.UploadedFileModel;
using WebApp.Models.Repositories;

namespace WebApp.Controllers;

/// <summary>
/// 影片控制器
/// 處理影片的播放、上傳、分片等功能
/// </summary>
public class VideoController : Controller
{
    private readonly IVideoService _service;

    private readonly IFileService _file_service;

    private readonly WebSocketService _socket_service;
    /// <summary>
    /// 建構函數
    /// </summary>
    /// <param name="service">影片服務</param>
    /// <param name="file_service">檔案服務</param>
    /// <param name="socket_service">WebSocket服務</param>
    public VideoController(IVideoService service, IFileService file_service, WebSocketService socket_service)
    {
        _service = service;
        _file_service = file_service;
        _socket_service = socket_service;

    }
    /// <summary>
    /// 顯示 Youtube IFrame 影片頁面
    /// </summary>
    public IActionResult Index()
    {
        return View();
    }
    /// <summary>
    /// 播放指定影片
    /// </summary>
    /// <param name="id">影片ID</param>
    /// <returns>影片播放頁面</returns>
    public async Task<IActionResult> Play(int id)
    {
        var video = await _file_service.Get(id);
        var path = video.FilePath;
        video.FilePath = path.Substring(path.IndexOf("wwwroot") + 7);
        ViewBag.Type = Path.GetExtension(video.FileName).Substring(1);
        return View(video);
    }

    /// <summary>
    /// 處理影片分片上傳
    /// </summary>
    /// <param name="chunk">影片分片檔案</param>
    /// <param name="fileId">檔案唯一識別碼</param>
    /// <param name="chunkIndex">分片索引</param>
    /// <returns>上傳結果</returns>
    [HttpPost]
    public async Task<IActionResult> Chunk(IFormFile chunk, string fileId, int chunkIndex)
    {
        if (chunk == null || string.IsNullOrEmpty(fileId))
            return BadRequest("Missing parameters");

        var path = await _service.UploadChunk(chunk, fileId, chunkIndex);

        return Ok(new { ok = true, savedAs = path });
    }

    /// <summary>
    /// 合併已上傳的影片分支
    /// </summary>
    /// <param name="fileId">檔案唯一識別碼</param>
    /// <param name="filename">檔案名稱</param>
    /// <param name="totalChunks">總分片數</param>
    /// <param name="userId">使用者ID</param>
    /// <returns>合併結果</returns>
    [HttpPost]
    public async Task<IActionResult> Merge(string fileId, string filename, int totalChunks, int userId)
    {
        var path = await _service.Merge(fileId, filename, totalChunks, userId);
        if (!System.IO.File.Exists(path))
        {
            return StatusCode(500, path);
        }
        path = path.Substring(path.IndexOf("wwwroot") + 7);
        return Ok(new { ok = true, output = path });
    }

    /// <summary>
    /// 檢查影片是否存在
    /// </summary>
    /// <param name="videoId">影片ID</param>
    /// <param name="userId">用戶ID</param>
    /// <returns>檢查結果</returns>
    public async Task<IActionResult> CheckVideo(string videoId, int userId)
    {
        return Json(new { success = await _service.CheckVideo(videoId, userId) });
    }

    /// <summary>
    /// 紀錄影片播放位置
    /// </summary>
    /// <param name="userId">用戶ID</param>
    /// <param name="videoId">影片ID</param>
    /// <param name="lp">最後播放位置(秒)</param>
    /// <returns>紀錄結果</returns>
    [HttpPost]
    public async Task<IActionResult> Record(int userId, string videoId, int lp)
    {
        var log = new PlayBackLog
        {
            UserId = userId,
            VideoId = videoId,
            LastPosition = lp,
            UpdateAt = DateTime.Now
        };

        await _service.Record(log);

        return Ok(new { ok = true });
    }
    
    /// <summary>
    /// 獲取影片播放紀錄
    /// </summary>
    /// <param name="userId">用戶ID</param>
    /// <param name="videoId">影片ID</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> GetLog(int userId, string videoId)
    {
        var log = await _service.GetLog(userId, videoId);
        return Ok(new
        {
            lastPosition = log?.LastPosition ?? 0
        });
    }
}