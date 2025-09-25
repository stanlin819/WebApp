using WebApp.Models.BudgetModel;

namespace WebApp.Models.Service;

public interface IBudgetService
{
    Task<IEnumerable<Budget>> GetAllBudgets();
    Task<Budget> GetBudget(int id);
    Task<String> AddBudget(Budget budget);
    Task<String> UpdateBudget(Budget budget);
    Task<String> DeleteBudget(int id);
    Task<IEnumerable<Budget>> GetUserBudgets(int userId);
    Task InitUserBudget(int userId);
}