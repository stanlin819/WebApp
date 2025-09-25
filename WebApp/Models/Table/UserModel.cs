using WebApp.Models.TransactionModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

using WebApp.Data;

using Microsoft.EntityFrameworkCore;
using WebApp.Models.UploadedFileModel;
using WebApp.Models.PlayBackLogModel;
using WebApp.Models.BudgetModel;
using WebApp.Models.TodoModel;

namespace WebApp.Models.UserModel;

public class User : IObject
{
    public int Id { get; set; }
    [Required(ErrorMessage="Username is required")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage="Invalid email")]
    public string Email { get; set; }

    [ValidateNever]
    public List<Transaction> Transactions { get; set; }
    [ValidateNever]
    public List<UploadedFile> UploadedFiles { get; set; }
    [ValidateNever]
    public List<PlayBackLog> PlayBackLogs { get; set; }
    [ValidateNever]
    public List<Budget> Budgets { get; set; }
    [ValidateNever]
    public List<Todo> Todos { get; set; }

    public User() { }

    public User(string username, string email)
    {
        Username = username;
        Email = email;
    }

    public override string ToString()
    {
        return $"UserId: {Id}, User: {Username}, Email: {Email}";
    }

}

public class UserViewModel
{
    public int UserId { get; set; }
    public string UserName { get; set; }
    public int TransactionCount { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public DateTime? LastTransactionDate { get; set; }
    public string Email{ get; set; }
}