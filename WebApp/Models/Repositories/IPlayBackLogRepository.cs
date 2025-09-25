using WebApp.Models.PlayBackLogModel;
namespace WebApp.Models.Repositories;

public interface IPlayBackLogRepository
{
    Task Add(PlayBackLog log);
    Task Delete(List<PlayBackLog> log);
    Task<PlayBackLog> Getlog(int userId, string videoId);
    Task Update(PlayBackLog log);
}