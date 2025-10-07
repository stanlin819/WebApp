using Microsoft.EntityFrameworkCore;
using WebApp.API.Data;
namespace WebApp.API.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    /// <summary>
    /// 初始化UserRepository 實例
    /// </summary>
    /// <param name="context">AppDbContext</param>
    public UserRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<User>> GetAll()
    {
        return await _context.Users
                    .Include(u => u.Todos) // 載入用戶的代辦事項集合
                    .ToListAsync();
    }

    public async Task<User> Get(int id)
    {
        return await _context.Users
            .Include(u => u.Todos)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User> GetByUserName(string userName)
    {
        var u = await _context.Users.FirstOrDefaultAsync(u => u.Username == userName);
        return u;
    }


    public async Task<User> Add(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<IEnumerable<User>> AddUsers(IEnumerable<User> users)
    {
        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();
        return users;
    }

    public async Task<User> Update(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<string> Delete(User user)
    {
        string n = user.Username;
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return n;
    }

    public async Task Delete(IEnumerable<User> users)
    {
        _context.Users.RemoveRange(users);
        await _context.SaveChangesAsync();
    }

}