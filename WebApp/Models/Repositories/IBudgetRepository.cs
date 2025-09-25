using WebApp.Models.BudgetModel;

namespace WebApp.Models.Repositories;

public interface IBudgetRepository : IRepository<Budget>
{
    Task<IEnumerable<Budget>> GetUserBudget(int userId);
}