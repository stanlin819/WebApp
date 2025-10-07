using WebApp.Models.UserModel;
namespace WebApp.Models.TransactionModel;

using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;


public class Transaction : IObject
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Please input Title")]
    public string Title { get; set; }

    [Required(ErrorMessage = "Amount is required")]
    [Range(0, double.MaxValue, ErrorMessage = "Amount must be greater than or equal to 0")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Date is required")]
    public DateTime Date { get; set; }

    [Required(ErrorMessage = "Please select Category")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid category.")] 
    public Category Category { get; set; } // 餐飲/交通/娛樂
    public bool IsIncome { get; set; }
    public int UserId { get; set; }
    [ValidateNever]
    [JsonIgnore]
    public User User { get; set; }

    public Transaction() { }

    public Transaction(int id, string title, decimal amount, Category category, bool isIncome, int userId)
    {
        Id = id;
        Title = title;
        Date = DateTime.Now;
        Category = category;
        IsIncome = isIncome;
        UserId = userId;
    }

    public void SetUserId(int userId)
    {
        UserId = userId;
    }

    public void SetIncome()
    {
        IsIncome = Category == Category.Work;
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
    public Category Category { get; set; }
    public bool IsIncome { get; set; }
    public string UserName { get; set; }
    public int Id { get; set; }
}

public enum Category{
    None = 0,
    Food,           // 餐飲
    Transport,      // 交通
    Entertainment,  // 娛樂
    Shopping,      // 購物
    Work,          // 工作
    Others         // 其他
}