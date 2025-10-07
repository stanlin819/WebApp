
using WebApp.API.Repositories;
namespace WebApp.API.Service;

public class UserService : IUserService
{
    private readonly IUserRepository _repo;

    /// <summary>
    /// 建構函式
    /// </summary>
    /// <param name="repo">用戶儲存庫的依賴注入</param>
    public UserService(IUserRepository repo)
    {
        _repo = repo;
    }

    /// <summary>
    /// 取得所有用戶及其相關資料
    /// </summary>
    /// <returns>包含所有用戶的集合</returns>
    public async Task<IEnumerable<User>> GetAllUsers()
    {
        return await _repo.GetAll();
    }

    /// <summary>
    /// 根據用戶 ID 取得特定用戶
    /// </summary>
    /// <param name="id">用戶 ID</param>
    /// <returns>找到的用戶物件，若不存在則返回 null</returns>
    public async Task<User> GetUser(int id)
    {
        return await _repo.Get(id);
    }

    /// <summary>
    /// 根據用戶名稱取得特定用戶
    /// </summary>
    /// <param name="userName">用戶名稱</param>
    /// <returns>找到的用戶物件，若不存在則返回 null</returns>
    public async Task<User> GetUserByUsername(string userName)
    {
        return await _repo.GetByUserName(userName);
    }

    /// <summary>
    /// 新增單一用戶，會檢查用戶名是否重複
    /// </summary>
    /// <param name="user">要新增的用戶物件</param>
    /// <returns>新增成功返回用戶物件，用戶名重複則返回 null</returns>
    public async Task<User> AddUser(User user)
    {
        // 檢查用戶名是否存在
        var existingUser = await GetUserByUsername(user.Username);
        if (existingUser != null)
        {
            return null; // 用戶名已存在
        }

        var addedUser = await _repo.Add(user);
        return addedUser;
    }

    /// <summary>
    /// 批量新增多個用戶，自動過濾重複的用戶名
    /// </summary>
    /// <param name="users">要新增的用戶集合</param>
    /// <returns>重複的用戶名稱列表，若無重複則返回空字串</returns>
    public async Task<string> AddUsers(IEnumerable<User> users)
    {
        var userList = users.ToList();
        if (!userList.Any())
        {
            return string.Empty;
        }

        // 取得所有現有用戶名稱
        var existingUsers = await _repo.GetAll();
        var existingUserNames = existingUsers.Select(u => u.Username).ToHashSet();

        //處理輸入用戶列表中的重複用戶名 (只保留第一個)
        var uniqueUsers = userList
            .GroupBy(u => u.Username)
            .Select(g => g.First())
            .ToList();

        //分離新用戶和重複用戶
        var newUsers = uniqueUsers
            .Where(u => !existingUserNames.Contains(u.Username))
            .ToList();

        var duplicateUsernames = uniqueUsers
            .Where(u => existingUserNames.Contains(u.Username))
            .Select(u => u.Username)
            .ToList();

        // 批量新增新用戶
        if (newUsers.Any())
        {
            await _repo.AddUsers(newUsers);
        }

        // 返回重複的用戶名稱
        return duplicateUsernames.Any()
            ? string.Join(", ", duplicateUsernames)
            : string.Empty;
    }

    /// <summary>
    /// 更新現有用戶資訊，會驗證用戶存在性和用戶名唯一性
    /// </summary>
    /// <param name="user">要更新的用戶物件</param>
    /// <returns>更新結果訊息</returns>
    public async Task<string> UpdateUser(User user)
    {
        // 檢查用戶是否存在
        var existingUser = await GetUser(user.Id);
        if (existingUser == null)
        {
            return "User not found";
        }

        //檢查用戶名是否已被其他用戶使用
        var userWithSameUserName = await GetUserByUsername(user.Username);
        if (userWithSameUserName != null && userWithSameUserName.Id != user.Id)
        {
            return "UserName already exists";
        }

        //更新用戶資訊
        existingUser.Username = user.Username;
        existingUser.Email = user.Email;

        var updatedUser = await _repo.Update(existingUser);
        return $"{updatedUser.Username} edited successfully.";
    }

    /// <summary>
    /// 刪除單一用戶
    /// </summary>
    /// <param name="id">要刪除的用戶 ID</param>
    /// <returns>刪除結果訊息</returns>
    public async Task<string> DeleteUser(int id)
    {
        var user = await _repo.Get(id);
        // 執行刪除操作
        var deletedUsername = await _repo.Delete(user);
        return $"{deletedUsername} deleted successfully";
    }

    /// <summary>
    /// 批量刪除多個用戶，會檢查每個用戶是否存在
    /// </summary>
    /// <param name="users">要刪除的用戶集合</param>
    /// <returns>刪除結果訊息，包含成功數量和失敗的用戶 ID</returns>
    public async Task<string> DeleteUsers(IEnumerable<User> users)
    {
        var userList = users?.ToList();
        if (userList == null || !userList.Any())
        {
            return "No users provided for deletion";
        }

        // 檢查所有用戶是否存在
        var existingUsers = new List<User>();
        var notFoundUsers = new List<string>();

        foreach (var user in userList)
        {
            var existingUser = await _repo.Get(user.Id);
            if (existingUser != null)
            {
                existingUsers.Add(existingUser);
            }
            else
            {
                notFoundUsers.Add($"ID: {user.Id}");
            }
        }

        // 執行批量刪除
        if (existingUsers.Any())
        {
            await _repo.Delete(existingUsers);
        }

        // 建立結果訊息
        var result = new List<string>();

        if (existingUsers.Any())
        {
            result.Add($"{existingUsers.Count} users deleted successfully");
        }

        if (notFoundUsers.Any())
        {
            result.Add($"Users not found: {string.Join(", ", notFoundUsers)}");
        }

        return string.Join(". ", result);
    }
    
}
