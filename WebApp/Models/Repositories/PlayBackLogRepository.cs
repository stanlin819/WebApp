using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Models.PlayBackLogModel;

namespace WebApp.Models.Repositories;

public class PlayBackLogRepository:IPlayBackLogRepository
{
    private readonly AppDbContext _context;

    public PlayBackLogRepository(AppDbContext context) => _context = context;
    public async Task Add(PlayBackLog log)
    {
        _context.PlayBackLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task Delete(List<PlayBackLog> logs)
    {
        int count = 0;
        foreach (var log in logs)
        {
            Console.WriteLine("Count: " + count++);
            Console.WriteLine("Video Name: " + log.VideoId);
        }
        _context.PlayBackLogs.RemoveRange(logs);
        await _context.SaveChangesAsync();
    }

    public async Task<PlayBackLog> Getlog(int userId, string videoId)
    {
        return await _context.PlayBackLogs.Where(l => l.UserId == userId && videoId.Equals(l.VideoId)).FirstOrDefaultAsync();
    }

    public async Task Update(PlayBackLog log)
    {
        _context.PlayBackLogs.Update(log);
        await _context.SaveChangesAsync();
    }
}