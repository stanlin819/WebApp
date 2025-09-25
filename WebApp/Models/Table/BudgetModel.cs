using WebApp.Models.UserModel;

namespace WebApp.Models.BudgetModel;
using System.Text.Json.Serialization;


public class Budget : IObject
{
    public int Id { get; set; }
    public string Category { get; set; }
    public decimal? Limit { get; set; }
    public int UserId { get; set; }
    [JsonIgnore]
    public User User { get; set; }

    public Budget() { }
    public Budget(int id, string category, decimal limit, int userId)
    {
        Id = id;
        Category = category;
        Limit = limit;
        UserId = userId;
    }

    public void setLimit(decimal? l)
    {
        Limit = l;
    }

    public override string ToString()
    {
        return $"Budget: {Category}, Limit: {Limit}, UserId: {UserId}";
    }
}