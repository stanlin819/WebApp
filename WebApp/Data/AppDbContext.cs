using Microsoft.EntityFrameworkCore;
using WebApp.Models.UserModel;
using WebApp.Models.BudgetModel;
using WebApp.Models.TransactionModel;
using WebApp.Models.UploadedFileModel;
using WebApp.Models.PlayBackLogModel;
using WebApp.Models.TodoModel;

namespace WebApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<User> Users { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Budget> Budgets { get; set; }
    public DbSet<UploadedFile> UploadedFiles { get; set; }
    public DbSet<PlayBackLog> PlayBackLogs { get; set; }
    
    public DbSet<Todo> Todos { get; set; }
}