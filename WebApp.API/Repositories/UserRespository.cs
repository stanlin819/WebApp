using Microsoft.EntityFrameworkCore;
using WebApp.API.Data;
namespace WebApp.API.Repositories;
/// <summary>
/// 用戶資料庫操作存儲庫，實作用戶相關的CRUD操作
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    
    /// <summary>
    /// 建構函式
    /// </summary>
    /// <param name="context">AppDbContext</param>
    public UserRepository(AppDbContext context) => _context = context;

    /// <summary>
    /// 取得所有用戶及其相關的待辦事項
    /// </summary>
    /// <returns>包含所有用戶及其待辦事項的集合</returns>
    public async Task<IEnumerable<User>> GetAll()
    {
        return await _context.Users
                    .Include(u => u.Todos) // 載入用戶的代辦事項集合
                    .ToListAsync();
    }

    /// <summary>
    /// 根據用戶 ID 取得特定用戶及其相關的待辦事項
    /// </summary>
    /// <param name="id">用戶 ID</param>
    /// <returns>找到的用戶物件，若不存在則返回 null</returns>
    public async Task<User> Get(int id)
    {
        return await _context.Users
            .Include(u => u.Todos)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    /// <summary>
    /// 根據用戶名稱取得特定用戶
    /// </summary>
    /// <param name="userName">用戶名稱</param>
    /// <returns>找到的用戶物件，若不存在則返回 null</returns>
    public async Task<User> GetByUserName(string userName)
    {
        var u = await _context.Users.FirstOrDefaultAsync(u => u.Username == userName);
        return u;
    }


    /// <summary>
    /// 新增單一用戶到資料庫
    /// </summary>
    /// <param name="user">要新增的用戶物件</param>
    /// <returns>新增後的用戶物件</returns>
    public async Task<User> Add(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return user;
    }

    /// <summary>
    /// 批量新增多個用戶到資料庫
    /// </summary>
    /// <param name="users">要新增的用戶集合</param>
    /// <returns>新增後的用戶集合</returns>
    public async Task<IEnumerable<User>> AddUsers(IEnumerable<User> users)
    {
        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();
        return users;
    }

    /// <summary>
    /// 更新現有用戶資訊
    /// </summary>
    /// <param name="user">要更新的用戶物件</param>
    /// <returns>更新後的用戶物件</returns>
    public async Task<User> Update(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }

    /// <summary>
    /// 刪除單一用戶
    /// </summary>
    /// <param name="user">要刪除的用戶物件</param>
    /// <returns>被刪除用戶的用戶名稱</returns>
    public async Task<string> Delete(User user)
    {
        string n = user.Username;
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return n;
    }

    /// <summary>
    /// 批量刪除多個用戶
    /// </summary>
    /// <param name="users">要刪除的用戶集合</param>
    public async Task Delete(IEnumerable<User> users)
    {
        _context.Users.RemoveRange(users);
        await _context.SaveChangesAsync();
    }

}