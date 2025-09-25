using WebApp.Models.UserModel;

namespace WebApp.Models.PlayBackLogModel;
using System.Text.Json.Serialization;


public class PlayBackLog
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string VideoId { get; set; }
    public int LastPosition { get; set; }
    public DateTime UpdateAt { get; set; }
    [JsonIgnore]
    public User User { get; set; }
}