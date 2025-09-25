using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models.Service;
using WebApp.Models.PlayBackLogModel;
using WebApp.Models.UploadedFileModel;
using WebApp.Models.Repositories;

namespace WebApp.Controllers;


public class VideoController : Controller
{
    private readonly IVideoService _service;

    private readonly IFileService _file_service;

    private readonly WebSocketService _socket_service;

    public VideoController(IVideoService service, IFileService file_service, WebSocketService socket_service)
    {
        _service = service;
        _file_service = file_service;
        _socket_service = socket_service;
        
    }
    public IActionResult Index()
    {
        return View();
    }
    public async Task<IActionResult> Play(int id)
    {
        var video = await _file_service.Get(id);
        var path = video.FilePath;
        video.FilePath = path.Substring(path.IndexOf("wwwroot") + 7);
        ViewBag.Type = Path.GetExtension(video.FileName).Substring(1);
        return View(video);
    }

    public IActionResult Upload()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Chunk(IFormFile chunk, string fileId, int chunkIndex)
    {
        if (chunk == null || string.IsNullOrEmpty(fileId))
            return BadRequest("Missing parameters");

        var path = await _service.UploadChunk(chunk, fileId, chunkIndex);

        return Ok(new { ok = true, savedAs = path });
    }

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

    public async Task<IActionResult> CheckVideo(string videoId, int userId)
    {
        return Json(new { success = await _service.CheckVideo(videoId, userId) });
    }

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