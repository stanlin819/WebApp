using System.Net.WebSockets;
using System.Text;
using System.Collections.Concurrent;
using WebApp.Models.Options;
using Microsoft.Extensions.Options;
using System.Text.Json;
using WebApp.Models.UploadedFileModel;
using WebApp.Models.Repositories;

public class WebSocketService
{
    // 儲存所有連線
    private readonly ConcurrentDictionary<string, WebSocket> _sockets = new();
    private readonly ConcurrentDictionary<string, FileStream> _files = new();
    private readonly string _uploadPath;

    public WebSocketService(IOptions<VideoSettingOptions> options)
    {
        _uploadPath = options.Value.UploadPath;
    }

    // 新增連線
    public string AddSocket(WebSocket socket)
    {
        var id = Guid.NewGuid().ToString();
        _sockets.TryAdd(id, socket);
        return id;
    }

    // 移除連線
    public async Task RemoveSocketAsync(string id)
    {
        if (_sockets.TryRemove(id, out var socket))
        {
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "連線關閉", CancellationToken.None);
        }
    }

    // 廣播訊息給所有連線
    public async Task BroadcastAsync(string message)
    {
        var buffer = Encoding.UTF8.GetBytes(message);
        foreach (var socket in _sockets.Values)
        {
            if (socket.State == WebSocketState.Open)
            {
                await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }

    // 處理單一連線的收發
    public async Task ReceiveAsync(string id, WebSocket socket)
    {
        var buffer = new byte[64 * 1024];
        var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        string videoId = "";
        string userId = "";

        while (!result.CloseStatus.HasValue)
        {
            if (result.MessageType == WebSocketMessageType.Binary)
            {
                await WriteChunk(videoId, buffer, result.Count);
            }
            else if (result.MessageType == WebSocketMessageType.Text)
            {
                // 前端傳來 JSON 告知 fileId 或完成訊息
                var msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
                var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(msg);
                if (data == null)
                    return;
                var action = data["action"].ToString();


                if (action.Equals("START"))
                {
                    userId = data["userId"].ToString();
                    videoId = data["fileId"].ToString();
                    var newId = StartFile(videoId, userId);
                    if (!newId.Equals(videoId))
                    {
                        videoId = newId;
                        var bytes = Encoding.UTF8.GetBytes(newId);
                        await socket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
                else if (action.Equals("END"))
                {
                    FinishFile(videoId, userId);
                }
            }

            result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        }

        await RemoveSocketAsync(id);
    }


    // 建立檔案流
    public string StartFile(string fileId, string userId)
    {
        userId = $"user_{userId}";
        var filePath = Path.Combine(_uploadPath, userId, fileId);
        if (!Directory.Exists(Path.Combine(_uploadPath, userId)))
        {
            Directory.CreateDirectory(Path.Combine(_uploadPath, userId));
        }

        int count = 1;
        string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileId);
        string extension = Path.GetExtension(fileId);
        while (System.IO.File.Exists(filePath))
        {
            fileId = $"{fileNameWithoutExt}({count++}){extension}";
            filePath = Path.Combine(_uploadPath, userId, fileId);
        }

        var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        _files[fileId] = fs;
        return fileId;
    }

    // 寫入 chunk
    public async Task WriteChunk(string fileId, byte[] buffer, int count)
    {
        if (_files.TryGetValue(fileId, out var fs))
        {
            await fs.WriteAsync(buffer, 0, count);
        }
    }

    // 關閉檔案
    public void FinishFile(string fileId, string userId)
    {
        if (_files.TryRemove(fileId, out var fs))
        {
            fs.Close();
        }
    }
}
