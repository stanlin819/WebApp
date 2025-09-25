using WebApp.Models.UserModel;
namespace WebApp.Models.TransactionModel;
using System.Text.Json.Serialization;


public class Transaction : IObject
{
    public int Id { get; set; }
    public string Title { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Category { get; set; } // 餐飲/交通/娛樂
    public bool IsIncome { get; set; }
    public int UserId { get; set; }
    [JsonIgnore]
    public User User { get; set; }

    public Transaction() { }

    public Transaction(int id, string title, decimal amount, string category, bool isincome, int userid)
    {
        Id = id;
        Title = title;
        Date = DateTime.Now;
        Category = category;
        IsIncome = isincome;
        UserId = userid;
    }

    public void SetUserId(int userId)
    {
        UserId = userId;
    }

    public void SerIncome()
    {
        if (Category.Equals("Work"))
        {
            IsIncome = true;
        }
        else
        {
            IsIncome = false;
        }
    }
    public override string ToString()
    {
        return $"Transaction: {Title}, Amount: {Amount}, Date: {Date}, Category: {Category}, IsIncome: {IsIncome}";
    }
}
public class TransactionViewModel
{
    public string Title { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Category { get; set; }
    public bool IsIncome { get; set; }
    public string UserName { get; set; }
    public int Id { get; set; }
}