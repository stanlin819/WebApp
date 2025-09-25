using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Models.UserModel;
namespace WebApp.Models.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<User>> GetAll()
    {
        return await _context.Users
                    .Include(u => u.Transactions)
                    .Include(u => u.Budgets)
                    .Include(u => u.PlayBackLogs)
                    .Include(u => u.UploadedFiles)
                    .Include(u => u.Todos)
                    .ToListAsync();
    }

    public async Task<User> Get(int id)
    {
        var u = await _context.Users.FindAsync(id);
        return u;
    }

    public async Task<User> GetByUsername(string userName)
    {
        var u = await _context.Users.FirstOrDefaultAsync(u => u.Username == userName);
        return u;
    }


    public async Task<String> Add(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return user.Username;
    }

    public async Task AddUsers(IEnumerable<User> users)
    {
        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();
    }

    public async Task<String> Update(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user.Username;
    }

    public async Task<String> Delete(int id)
    {
        var u = await _context.Users.FindAsync(id);
        if (u != null)
        {
            _context.Users.Remove(u);
            await _context.SaveChangesAsync();
            return u.Username;
        }
        return "";
    }

    public async Task Delete(IEnumerable<User> users)
    {
        _context.Users.RemoveRange(users);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<UserViewModel>> GetUserOverview()
    {
        return await (from u in _context.Users
                      join t in _context.Transactions on u.Id equals t.UserId into userOverviews
                      select new UserViewModel
                      {
                          UserId = u.Id,
                          UserName = u.Username,
                          TransactionCount = userOverviews.Count(),
                          TotalIncome = userOverviews
                              .Where(t => t.IsIncome)
                              .Sum(t => (decimal?)t.Amount) ?? 0,
                          TotalExpense = userOverviews
                              .Where(t => !t.IsIncome)
                              .Sum(t => (decimal?)t.Amount) ?? 0,
                          LastTransactionDate = userOverviews
                              .Max(t => (DateTime?)t.Date),
                          Email = u.Email
                      }).ToListAsync();

    }
}