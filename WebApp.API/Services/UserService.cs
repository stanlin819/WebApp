
using WebApp.API.Repositories;
namespace WebApp.API.Service;

public class UserService : IUserService
{
    private readonly IUserRepository _repo;

    public UserService(IUserRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<User>> GetAllUsers()
    {
        return await _repo.GetAll();
    }

    public async Task<User> GetUser(int id)
    {
        return await _repo.Get(id);
    }

    public async Task<User> GetUserByUsername(string userName)
    {
        return await _repo.GetByUserName(userName);
    }

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
        return $"{updatedUser.Username} edited successfuly.";
    }

    public async Task<string> DeleteUser(int id)
    {
        var user = await _repo.Get(id);
        // 執行刪除操作
        var deletedUsername = await _repo.Delete(user);
        return $"{deletedUsername} deleted successfuly";
    }

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
