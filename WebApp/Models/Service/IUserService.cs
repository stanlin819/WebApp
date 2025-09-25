using WebApp.Models.UserModel;

namespace WebApp.Models.Service;

public interface IUserService
{
    Task<IEnumerable<User>> GetAllUsers();
    Task<User> GetUser(int id);
    Task<User> GetUserByUsername(string userName);
    Task<Dictionary<string, string>> AddUser(User user);
    Task<Dictionary<string,string>> AddUsers(IEnumerable<User> users);
    Task<Dictionary<string,string>> UpdateUser(User user);
    Task<String> DeleteUser(int id);
    Task DeleteUsers(IEnumerable<User> users);
    Task<IEnumerable<UserViewModel>> GetUserOverview();
}
