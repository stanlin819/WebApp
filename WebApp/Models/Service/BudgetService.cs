using WebApp.Models.BudgetModel;
using WebApp.Models.Repositories;

namespace WebApp.Models.Service;

public class BudgetService : IBudgetService
{
    private readonly IBudgetRepository _repo;

    public BudgetService(IBudgetRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<Budget>> GetAllBudgets()
    {
        return await _repo.GetAll();
    }

    public async Task<Budget> GetBudget(int id)
    {
        return await _repo.Get(id);
    }

    public async Task<String> AddBudget(Budget budget)
    {
        return await _repo.Add(budget);
        
    }

    public async Task InitUserBudget(int id)
    {
        var Cate = new List<String>{"Shopping", "Food", "Transport", "Entertainment", "Other"};
        foreach (var category in Cate)
        {
            var budget = new Budget
            {
                Category = category,
                Limit = null,
                UserId = id,
            };
            await AddBudget(budget);
        }
    }

    public async Task<String> UpdateBudget(Budget budget)
    {
        return await _repo.Update(budget);
    }

    public async Task<String> DeleteBudget(int id)
    {
        return await _repo.Delete(id);
    }

    public async Task<IEnumerable<Budget>> GetUserBudgets(int userId)
    {
        return await _repo.GetUserBudget(userId);
    }
}