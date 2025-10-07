
using WebApp.API.Repositories;
namespace WebApp.API.Service;

public class UserService
{
    private readonly UserRepository _repo;

    public UserService(UserRepository repo)
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
        return await _repo.GetByUsername(userName);
    }

    public async Task<Dictionary<string, string>> AddUser(User user)
    {
        var mes = new Dictionary<string, string>();
        var existingUsers = await _repo.GetAll();
        var existingUsernames = existingUsers.Select(u => u.Username).ToHashSet();
        if (existingUsernames.Contains(user.Username))
        {
            mes["isSuccess"] = "false";
            mes["Message"] = $"{user.Username} already exists.";
        }
        else
        {
            await _repo.Add(user);
            mes["isSuccess"] = "true";
            mes["Message"] = $"{user.Username} created successfully.";
        }
        return mes;
    }
    public async Task<Dictionary<string,string>> AddUsers(IEnumerable<User> users)
    {
        var mes = new Dictionary<string, string>();

        var existingUsers = await _repo.GetAll();
        var existingUsernames = existingUsers.Select(u => u.Username).ToHashSet();

         var groupedUser = users
            .GroupBy(u => u.Username)
            .ToList();

        var duplicateUsers = groupedUser
            .Where(g => existingUsernames.Contains(g.Key))
            .Select(g => $"{g.Key} ({g.First().Email}) [existing]")
            .ToList();
        
        var newUsers = groupedUser
            .Where(g => !existingUsernames.Contains(g.Key))
            .Select(g => g.First())
            .ToList();

        await _repo.AddUsers(newUsers);

        if (duplicateUsers.Any())
        {
            mes["Warning"] = $"Found {duplicateUsers.Count} duplicate users: {string.Join(", ", duplicateUsers)}";
        }

        mes["Message"] = $"{newUsers.Count()} users imported successfully.";

        return mes;
    }
    public async Task<Dictionary<string,string>> UpdateUser(User user)
    {
        var mes = new Dictionary<string, string>();
        try
        {
            var existingUser = await GetUser(user.Id);

            var otherUsers = await GetAllUsers();
            if (otherUsers.Any(u => u.Id != user.Id && u.Username.Equals(user.Username)))
            {
                mes["isSuccess"] = "false";
                mes["Message"] = "Username already exists";
                return mes;
            }

            existingUser.Username = user.Username;
            existingUser.Email = user.Email;

            var name = await _repo.Update(existingUser);
            mes["isSuccess"] = "true";
            mes["Message"] = $"{name} edited successfully.";
        }
        catch (Exception ex)
        {
            mes["isSuccess"] = "false";
            mes["Message"] = $"Error updating user: {ex.Message}";
        }

        return mes;
    }

    public async Task<String> DeleteUser(int id)
    {
        return await _repo.Delete(id);
    }

    public async Task DeleteUsers(IEnumerable<User> users)
    {
        await _repo.Delete(users);
    }
    
}
