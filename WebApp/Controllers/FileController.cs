using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebApp.Models.Options;
using System.IO.Compression;
using WebApp.Models.Service;

public class FileController : Controller
{
    private readonly IFileService _service;
    public FileController(IFileService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> UploadFile(List<IFormFile> files, int userId, int page)
    {
        var message = await _service.UploadFile(files, userId);
        if (message != "successfully")
        {
            TempData["Message"] = message;
            TempData["isSuccess"] = false;
        }
        else
        {
            TempData["isSuccess"] = true;
            TempData["Message"] = "Files uploaded successfully";

        }
        return RedirectToAction("Details", "User", new { id = userId, currentPage = page });
    }

    [HttpGet]
    public IActionResult DownloadFile(string fileName, int userId)
    {
        var fileBytes = _service.DownloadFile(fileName, userId);

        var contentType = "application/octet-stream";

        return File(fileBytes, contentType, fileName);
    }

    [HttpPost]
    public IActionResult DownloadMultipleFiles(string[] selectedFiles, int userId)
    {
        if (selectedFiles.Count() > 0)
        {
            var memoryStream = _service.DownloadMultipleFiles(selectedFiles, userId);

            return File(memoryStream.ToArray(), "application/zip", "SelectedFiles.zip");
        }
        TempData["Message"] = "Select at least one file";
        TempData["isSuccess"] = false;
        return RedirectToAction("Details", "User", new { id = userId });

    }

    public async Task<IActionResult> DeleteFile(string fileName, int userId, int page)
    {
        var dl = new List<string>();
        dl.Add(fileName);
        var message = await _service.RemoveFiles(dl, userId);
        if (message != "successfully")
        {
            TempData["isSuccess"] = false;
            TempData["Message"] = message;
        }
        else
        {
            TempData["isSuccess"] = true;
            TempData["Message"] = $"{fileName} deleted successfuly";
        }
        return RedirectToAction("Details", "User", new { id = userId, currentPage = page });
    }

    public async Task<IActionResult> DeleteVideo(string fileName, int userId)
    {
        var dl = new List<string>();
        dl.Add(fileName);
        var message = await _service.RemoveFiles(dl, userId);
        if (message != "successfully")
        {
            return Ok(new { ok = false, mess = message});
        }
        else
        {
            return Ok(new { ok = true, mess = message});
        }
    }

    public async Task<IActionResult> DeleteMultipleFiles(string[] selectedFiles, int userId, int page)
    {
        if (selectedFiles.Count() == 0)
        {
            TempData["isSuccess"] = false;
            TempData["Message"] = "At least select one file";
        }
        else
        {
            await _service.RemoveFiles(selectedFiles, userId);
        }
        return RedirectToAction("Details", "User", new { id = userId, currentPage = page });
    }

}