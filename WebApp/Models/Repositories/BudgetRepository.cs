using WebApp.Models.BudgetModel;
using WebApp.Data;
using AspNetCoreGeneratedDocument;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Models.Repositories;

public class BudgetRepository : IBudgetRepository
{
    private readonly AppDbContext _context;

    public BudgetRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Budget>> GetAll()
    {
        return await _context.Budgets.ToListAsync();
    }
    public async Task<IEnumerable<Budget>> GetUserBudget(int userId) =>
        await _context.Budgets.Where(b => b.UserId == userId).ToListAsync();

    public async Task<Budget> Get(int id)
    {
        var b = await _context.Budgets.FindAsync(id);
        if (b == null)
            throw new KeyNotFoundException($"Budget with id {id} not found.");
        else
            return b;         
    }


    public async Task<String> Add(Budget budget)
    {
        _context.Add(budget);
        await _context.SaveChangesAsync();
        return budget.Category;
        
    }
    public async Task<String> Update(Budget budget)
    {
        _context.Update(budget);
        await _context.SaveChangesAsync();
        return budget.Category;
    }
    public async Task<String> Delete(int id)
    {
        var b = await _context.Budgets.FindAsync(id);
        if (b != null)
        {
            _context.Budgets.Remove(b);
            await _context.SaveChangesAsync();
            return b.Category;
        }
        return "";
    }
}